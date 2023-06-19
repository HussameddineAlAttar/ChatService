using ChatService.DTO;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Services;

public class ImageService : IImageService
{
    private readonly IImageStore imageStore;

    public ImageService(IImageStore _imageStore)
    {
        imageStore = _imageStore;
    }

    public async Task<byte[]> DownloadImage(string id)
    {
        var imageStream = await imageStore.DownloadImage(id);
        byte[] contentBytes;
        using (var ms = new MemoryStream())
        {
            imageStream.CopyTo(ms);
            contentBytes = ms.ToArray();
        }
        return contentBytes;
    }

    public async Task UploadImage([FromForm] UploadImageRequest request, string username)
    {
        await imageStore.UploadImage(request, username);
    }
}
