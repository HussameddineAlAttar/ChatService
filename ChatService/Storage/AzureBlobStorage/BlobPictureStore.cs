using ChatService.DTO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ChatService.Exceptions;

namespace ChatService.Storage.AzureBlobStorage;
public class BlobPictureStore : IImageStore
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobPictureStore(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    private BlobContainerClient Container => _blobServiceClient.GetBlobContainerClient("profilepictures");

    public async Task<Stream> DownloadImage(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException("Id cannot be null or empty");
        }
        BlobClient blobClient = Container.GetBlobClient(id + ".png");
        if (!await blobClient.ExistsAsync())
        {
            throw new ImageNotFoundException($"Image of id {id} not found.");
        }
        BlobDownloadResult content = await blobClient.DownloadContentAsync();
        var stream = content.Content.ToStream();
        return stream;
    }


    public async Task UploadImage(UploadImageRequest request, string username)
    {
        string pictureID = username;
        var blobName = $"{pictureID}.png";
        var blobClient = Container.GetBlobClient(blobName);
        var file = request.File;
        await blobClient.UploadAsync(file.OpenReadStream(), true);
    }

    public async Task DeleteImage(string? id)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException("Id cannot be null or empty");
        }
        BlobClient blobClient = Container.GetBlobClient(id + ".png");
        if (!await blobClient.ExistsAsync()) // if {id}.png doesn't exist
        {
            throw new ImageNotFoundException($"Image of id {id} not found.");
        }
        await blobClient.DeleteAsync();
        return;
    }
}