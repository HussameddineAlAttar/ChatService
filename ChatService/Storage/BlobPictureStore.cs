using System.Net;
using ChatService.DTO;
using Microsoft.Azure.Cosmos;
using ChatService.Storage.Entities;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;

namespace ChatService.Storage;
public class BlobPictureStore : IImageInterface
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobPictureStore(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    private BlobContainerClient Container => _blobServiceClient.GetBlobContainerClient("profilepictures");

    public async Task<Image?> DownloadImage(string id)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException("Id cannot be null or empty");
        }
        try
        {
            string type;

            BlobClient blobClient = Container.GetBlobClient(id + ".png");
            type = "png";
            if (!await blobClient.ExistsAsync()) // if {id}.png doesn't exist
            {
                blobClient = Container.GetBlobClient(id + ".jpeg");
                type = "jpeg";
                if (!await blobClient.ExistsAsync()) // if {id}.jpeg doesn't exist
                {
                    return null;
                }
            
            }

            BlobDownloadResult content = await blobClient.DownloadContentAsync();
            var stream = content.Content.ToStream();
            return new Image(stream, "image/" + type);

        }
        catch
        {
            throw;
        }
    }


    public async Task<string> UploadImage(UploadImageRequest request)
    {
        string pictureID = Guid.NewGuid().ToString();
        var file = request.File;
        string type = Path.GetExtension(file.FileName);
        if(type == ".jpg")
        {
            type = ".jpeg";
        }
        var blobName = $"{pictureID}{type}"; //file type to include in Image during download
        var blobClient = Container.GetBlobClient(blobName);
        await blobClient.UploadAsync(file.OpenReadStream(), true);
        return pictureID;
        
    }

    public async Task DeleteImage(string? id)
    {
        if(string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentNullException();
        }
        try
        {
            string type;

            BlobClient blobClient = Container.GetBlobClient(id + ".png");
            type = "png";
            if (!await blobClient.ExistsAsync()) // if {id}.png doesn't exist
            {
                blobClient = Container.GetBlobClient(id + ".jpeg");
                type = "jpeg";
                if (!await blobClient.ExistsAsync()) // if {id}.jpeg doesn't exist
                {
                    return;
                }
            
            }
            await blobClient.DeleteAsync();
            return;
        }
        catch
        {
            throw;
        }
    }
}