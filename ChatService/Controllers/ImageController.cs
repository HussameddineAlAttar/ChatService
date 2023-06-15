using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChatService.Controllers;

[ApiController]
[Route("api/images")]
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
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        await imageService.UploadImage(request);

        string imgID = request.username;

        telemetryClient.TrackMetric("ImageStore.AddImage.Time", stopwatch.ElapsedMilliseconds);
        logger.LogInformation("Uploaded image {ImageID}", imgID);

        var response = new UploadImageResponse(imgID);
        return CreatedAtAction(nameof(UploadImage), response);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult> DownloadImage(string username)
    {
        using (logger.BeginScope("{ImageID}", username))
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var imageBytes = await imageService.DownloadImage(username);

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