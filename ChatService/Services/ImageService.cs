using ChatService.DTO;
using ChatService.Extensions;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Services;

public class ImageService : IImageService
{
    private readonly IImageStore imageStore;
    private readonly IProfileStore profileStore;

    public ImageService(IImageStore _imageStore, IProfileStore _profileStore)
    {
        imageStore = _imageStore;
        profileStore = _profileStore;
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
        await profileStore.GetProfile(username); //checking if profile exists
        await imageStore.UploadImage(request, username.HashSHA256());
    }
}
