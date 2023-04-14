using ChatService.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Services;

public interface IImageService
{
    Task<string> UploadImage([FromForm] UploadImageRequest request);
    Task<byte[]> DownloadImage(string id);
}
