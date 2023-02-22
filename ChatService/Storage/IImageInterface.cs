using ChatService.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Storage;

public interface IImageInterface
{
    Task UploadImage([FromForm] UploadImageRequest request);
    Task<Image?> DownloadImage(string id);
    Task DeleteImage(string id);
}
