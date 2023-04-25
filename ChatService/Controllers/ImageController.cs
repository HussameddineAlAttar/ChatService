using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers;

[ApiController]
[Route("api/images")]
public class ImageController : ControllerBase
{
    private readonly IImageService imageService;
    private readonly ILogger<ImageController> logger;

    public ImageController(IImageService _imageService, ILogger<ImageController> _logger)
    {
        imageService = _imageService;
        logger = _logger;
    }

    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        var imgID = await imageService.UploadImage(request);
        var response = new UploadImageResponse(imgID);
        logger.LogInformation("Uploaded image {ImageID}", imgID);
        return CreatedAtAction(nameof(UploadImage), response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> DownloadImage(string id)
    {
        using (logger.BeginScope("{ImageID}", id))
        {
            try
            {
                var imageBytes = await imageService.DownloadImage(id);
                logger.LogInformation("Downloaded image");
                return new FileContentResult(imageBytes, "image/png");
            }
            catch (Exception e)
            {
                if (e is ImageNotFoundException)
                {
                    return NotFound($"Image of id {id} not found.");
                }
                throw;
            }
        }
    }
}