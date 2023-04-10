using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ChatService.Configuration;
using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Tests.ImageTests;

public class BlobStorageTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IImageStore blobStorage;


    public BlobStorageTest(WebApplicationFactory<Program> factory)
    {
        blobStorage = factory.Services.GetRequiredService<IImageStore>();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;

    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    public async Task DownloadNull_or_EmptyImage(string? invalid)
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await blobStorage.DownloadImage(invalid);
        });
    }

    [Fact]
    public async Task DownloadNonExistingImage()
    {
        await Assert.ThrowsAsync<ImageNotFoundException>(async () =>
        {
            await blobStorage.DownloadImage("randomID_doesn't_exist");
        });
    }

    [Theory]
    [InlineData("png")]
    [InlineData("jpeg")]
    public async Task UpoadImage(string type)
    {
        var stream = new MemoryStream();
        Image testImage = new Image(stream, type);
        FormFile file = new FormFile(stream, 0, stream.Length, null, "randomFileName." + type)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/" + type
        };

        UploadImageRequest request = new(file);
        var response = await blobStorage.UploadImage(request);

        Image image = await blobStorage.DownloadImage(response);

        using (var expectedContent = new MemoryStream())
        using (var actualContent = new MemoryStream())
        {
            testImage.Content?.CopyTo(expectedContent);
            image.Content?.CopyTo(actualContent);
            Assert.Equal(expectedContent.ToArray(), actualContent.ToArray());
        }

        await blobStorage.DeleteImage(response);
        
    }

    [Theory]
    [InlineData("png")]
    [InlineData("jpeg")]
    [InlineData("jpg")]
    public async Task Delete_Image(string type)
    {
        var stream = new MemoryStream();
        FormFile file = new FormFile(stream, 0, stream.Length, null, "randomFileName." + type)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/" + type
        };

        UploadImageRequest request = new(file);
        var response = await blobStorage.UploadImage(request);


        await blobStorage.DeleteImage(response);

        await Assert.ThrowsAsync<ImageNotFoundException>(async () =>
        {
            await blobStorage.DownloadImage(response);
        });

    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    public async Task DeleteNull_or_EmptyImage(string? invalid)
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await blobStorage.DeleteImage(invalid);
        });
    }


    [Fact]
    public async Task Delete_Image_NotFound()
    {
        await Assert.ThrowsAsync<ImageNotFoundException>(async () =>
        {
            await blobStorage.DeleteImage(Guid.NewGuid().ToString());
        });

    }
}

