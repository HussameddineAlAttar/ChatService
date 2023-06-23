using ChatService.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Storage;

public interface IImageStore
{
    Task UploadImage([FromForm] UploadImageRequest request, string id);
    Task<Stream> DownloadImage(string id);
    Task DeleteImage(string id);
}
