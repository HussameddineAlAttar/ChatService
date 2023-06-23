using ChatService.DTO;
using ChatService.Services;
using ChatService.Storage;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ChatService.Tests.ImageTests;

public class ImageServiceTest
{
    private readonly Mock<IImageStore> imageStoreMock = new();
    private readonly ImageService imageService;
    private readonly UploadImageRequest uploadImageRequest;
    private readonly string imageId;
    private readonly Stream testStream;
    private readonly byte[] imageBytes;
    private readonly FormFile file;

    public ImageServiceTest()
    {
        imageService = new ImageService(imageStoreMock.Object);
        testStream = new MemoryStream();
        using (var ms = new MemoryStream())
        {
            testStream.CopyTo(ms);
            imageBytes = ms.ToArray();
        }
        file = new FormFile(testStream, 0, testStream.Length, null, "randomFileName.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
        uploadImageRequest = new(file);
        imageId = Guid.NewGuid().ToString();
    }

    [Fact]
    public async Task DownloadImage()
    {
        imageStoreMock.Setup(m => m.DownloadImage(imageId)).ReturnsAsync(testStream);
        var actualImageBytes = await imageService.DownloadImage(imageId);
        Assert.Equal(imageBytes, actualImageBytes);
    }
}