using ChatService.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Services;

public interface IImageService
{
    Task UploadImage([FromForm] UploadImageRequest request, string username);
    Task<byte[]> DownloadImage(string id);
}
