using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ChatService.Extensions;

namespace ChatService.Controllers;

[ApiController]
[Route("api/images/{username}")]
public class ImageController : ControllerBase
{
    private readonly IImageService imageService;
    private readonly ILogger<ImageController> logger;
    private readonly TelemetryClient telemetryClient;

    public ImageController(IImageService _imageService, ILogger<ImageController> _logger, TelemetryClient _telemetryClient)
    {
        imageService = _imageService;
        logger = _logger;
        telemetryClient = _telemetryClient;
    }

    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request, string username)
    {
        try
        {
            var type = request.File.ContentType;
            if (type != "image/png" && type != "image/jpeg")
            {
                return BadRequest($"{type} file not supported. Upload a PNG or JPEG image instead.");
            }
            var stopwatch = Stopwatch.StartNew();
            await imageService.UploadImage(request, username);

            string imgID = username;

            telemetryClient.TrackMetric("ImageStore.AddImage.Time", stopwatch.ElapsedMilliseconds);
            logger.LogInformation("Uploaded image {ImageID}", imgID);

            var response = new UploadImageResponse(imgID);
            return CreatedAtAction(nameof(UploadImage), response);
        }
        catch (Exception ex)
        {
            if(ex is ProfileNotFoundException)
            {
                return NotFound($"User with username {username} not found.");
            }
            throw;
        }
    }

    [HttpGet]
    public async Task<ActionResult> DownloadImage(string username)
    {
        using (logger.BeginScope("{ImageID}", username))
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var imageBytes = await imageService.DownloadImage(username.HashSHA256());

                telemetryClient.TrackMetric("ImageStore.GetImage.Time", stopwatch.ElapsedMilliseconds);
                logger.LogInformation("Downloaded image");

                return new FileContentResult(imageBytes, "image/png");
            }
            catch (Exception e)
            {
                if (e is ImageNotFoundException)
                {
                    return NotFound($"Image for user {username} not found.");
                }
                throw;
            }
        }
    }
}