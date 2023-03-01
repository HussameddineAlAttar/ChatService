﻿using Azure.Storage.Blobs.Models;
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
        var image = await imageInterface.DownloadImage(id);
        if (image == null)
        {
            return NotFound($"Image of id {id} not found.");
        }

        byte[] contentBytes;
        using (var ms = new MemoryStream())
        {
            image.Content.CopyTo(ms);
            contentBytes = ms.ToArray();
        }
        return File(contentBytes, image.ContentType);
    }

    //[HttpDelete("{id}")]
    //public async Task<IActionResult> DeleteImage(string id)
    //{
    //    var image = await imageInterface.DownloadImage(id);
    //    if (image == null)
    //    {
    //        return NotFound($"Image of id {id} not found");
    //    }
    //    await imageInterface.DeleteImage(id);
    //    return Ok($"Image of id {id} successfully deleted");
    //}
}