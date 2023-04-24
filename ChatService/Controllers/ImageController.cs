using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/images")]
public class ImageController : ControllerBase
{
    private readonly IImageService imageService;

    public ImageController(IImageService _imageService)
    {
        imageService = _imageService;
    }

    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        var imgID = await imageService.UploadImage(request);
        var response = new UploadImageResponse(imgID);
        return CreatedAtAction(nameof(UploadImage), response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> DownloadImage(string id)
    {
        try
        {
            var imageBytes = await imageService.DownloadImage(id);
            return new FileContentResult(imageBytes, "image/png");
        }
        catch(Exception e)
        { 
            if(e is ImageNotFoundException)
            {
                return NotFound($"Image of id {id} not found.");
            }
            throw;
        }
    }
}