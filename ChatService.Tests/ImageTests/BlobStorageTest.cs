using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Tests.ImageTests;

public class BlobStorageTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IImageStore blobStorage;
    private readonly string username;


    public BlobStorageTest(WebApplicationFactory<Program> factory)
    {
        blobStorage = factory.Services.GetRequiredService<IImageStore>();
        username = "FooBar" + Guid.NewGuid().ToString();
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
        var testStream = new MemoryStream();
        FormFile file = new FormFile(testStream, 0, testStream.Length, null, "randomFileName." + type)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/" + type
        };

        UploadImageRequest request = new(file, username);
        await blobStorage.UploadImage(request);

        Stream imageStream = await blobStorage.DownloadImage(username);

        using (var expectedContent = new MemoryStream())
        using (var actualContent = new MemoryStream())
        {
            imageStream.CopyTo(expectedContent);
            imageStream.CopyTo(actualContent);
            Assert.Equal(expectedContent.ToArray(), actualContent.ToArray());
        }

        await blobStorage.DeleteImage(username);
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

        UploadImageRequest request = new(file, username);
        await blobStorage.UploadImage(request);
        await blobStorage.DeleteImage(username);

        await Assert.ThrowsAsync<ImageNotFoundException>(async () =>
        {
            await blobStorage.DownloadImage(username);
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