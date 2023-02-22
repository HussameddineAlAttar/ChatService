using Azure.Storage.Blobs.Models;
using ChatService.DTO;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("images")]
public class ImageController : ControllerBase
{
    private readonly IProfileInterface profileInterface;
    private readonly IImageInterface imageInterface;

    public ImageController(IProfileInterface _profileInterface, IImageInterface _imageInterface)
    {
        profileInterface = _profileInterface;
        imageInterface = _imageInterface;
    }

    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        var type = request.File.ContentType;
        if (type != "image/png" && type != "image/jpg" && type != "image/jpeg")
        {
            return BadRequest("Image file type not supported. Upload PNG, JPG, or JPEG.");
        }
        await imageInterface.UploadImage(request);
        var response = new UploadImageResponse(request.id);
        return Ok(response);

    }

    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadImage(string id)
    {
        var image = await imageInterface.DownloadImage(id);
        if (image == null)
        {
            return NotFound($"Image of id {id} not found.");
        }
        return File(image.Content, image.ContentType);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImage(string id)
    {
        var image = await imageInterface.DownloadImage(id);
        if(image == null)
        {
            return NotFound($"Image of id {id} not found");
        }
        await imageInterface.DeleteImage(id);
        return Ok($"Image of id {id} successfully deleted");
    }
}