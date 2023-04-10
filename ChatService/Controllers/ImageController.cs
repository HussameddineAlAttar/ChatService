using Azure.Storage.Blobs.Models;
using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("images")]
public class ImageController : ControllerBase
{
    private readonly IImageStore imageInterface;

    public ImageController(IImageStore _imageInterface)
    {
        imageInterface = _imageInterface;
    }

    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        var type = request.File.ContentType;
        if (type != "image/png" && type != "image/jpeg")
        {
            return BadRequest("Image file type not supported. Upload PNG or JPEG.");
        }
        var imgID = await imageInterface.UploadImage(request);
        var response = new UploadImageResponse(imgID);
        return CreatedAtAction(nameof(UploadImage), response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> DownloadImage(string id)
    {
        try
        {
            var image = await imageInterface.DownloadImage(id);
            byte[] contentBytes;
            using (var ms = new MemoryStream())
            {
                image.Content.CopyTo(ms);
                contentBytes = ms.ToArray();
            }
            return File(contentBytes, image.ContentType);
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