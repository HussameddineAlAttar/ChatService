﻿using ChatService.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Storage;

public interface IImageInterface
{
    Task<string> UploadImage([FromForm] UploadImageRequest request);
    Task<Image?> DownloadImage(string id);
    Task DeleteImage(string id);
}
