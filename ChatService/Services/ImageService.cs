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
        try
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
        catch
        {
            throw;
        }
    }

    public async Task<string> UploadImage([FromForm] UploadImageRequest request)
    {
        try
        {
            string imageId = await imageStore.UploadImage(request);
            return imageId;
        }
        catch
        {
            throw;
        }
    }
}
