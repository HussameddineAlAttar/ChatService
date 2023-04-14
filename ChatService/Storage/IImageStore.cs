﻿using ChatService.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Storage;

public interface IImageStore
{
    Task<string> UploadImage([FromForm] UploadImageRequest request);
    Task<Stream> DownloadImage(string id);
    Task DeleteImage(string id);
}
