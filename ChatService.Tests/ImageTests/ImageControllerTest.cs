using ChatService.DTO;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace ChatService.Tests.ImageTests;

public class ImageControllerTest: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IProfileInterface> profileStoreMock = new();
    private readonly Mock<IImageInterface> blobStorageMock = new();
    private readonly Mock<IFormFile> fileMock = new();
    private readonly HttpClient httpClient;
    private readonly ImageController controller;

    public ImageControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(profileStoreMock.Object); });
            builder.ConfigureTestServices(services => { services.AddSingleton(blobStorageMock.Object); });
        }).CreateClient();
        controller = new ImageController(profileStoreMock.Object, blobStorageMock.Object);
}

    [Theory]
    [InlineData("image/png")]
    [InlineData("image/jpg")]
    [InlineData("image/jpeg")]
    public async Task UploadValidImage(string validType)
    {
        var fileName = "test.png";
        var content = "random content string";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;

        fileMock.Setup(mock => mock.FileName).Returns(fileName);
        fileMock.Setup(mock => mock.Length).Returns(ms.Length);
        fileMock.Setup(mock => mock.OpenReadStream()).Returns(ms);
        fileMock.Setup(mock => mock.ContentType).Returns(validType);

        var request = new UploadImageRequest(fileMock.Object, "test-id");

        blobStorageMock.Setup(mock => mock.UploadImage(It.IsAny<UploadImageRequest>())).Returns(Task.CompletedTask);

        var result = await controller.UploadImage(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UploadImageResponse>(okResult.Value);
        Assert.Equal("test-id", response.Id);
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("video/mp4")]
    public async Task UploadNonImage_Bad(string invalidType)
    {
        fileMock.Setup(mock => mock.ContentType).Returns(invalidType);

        var request = new UploadImageRequest(fileMock.Object, "test-id");

        blobStorageMock.Setup(mock => mock.UploadImage(It.IsAny<UploadImageRequest>())).Returns(Task.CompletedTask);

        var result = await controller.UploadImage(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Image file type not supported. Upload PNG, JPG, or JPEG.", badRequestResult.Value);
    }


    [Fact]
    public async Task DownloadValidImage()
    {
        var id = "test-id";
        var contentType = "image/png";
        var content = "random content string";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;
        var image = new Image(ms, contentType);
        blobStorageMock.Setup(mock => mock.DownloadImage(id)).ReturnsAsync(image);

        var result = await controller.DownloadImage(id);

        Assert.IsType<FileStreamResult>(result);

        var fileStreamResult = result as FileStreamResult;
        Assert.Equal(contentType, fileStreamResult.ContentType);

        var responseStream = new MemoryStream();
        await fileStreamResult.FileStream.CopyToAsync(responseStream);
        responseStream.Position = 0;
        Assert.Equal(ms.Length, responseStream.Length);
        Assert.Equal(ms.ToArray(), responseStream.ToArray());

    }


    [Fact]
    public async Task DownloadImage_NotFound()
    {
        string randomID = "randomID_doesn't_exist";
        blobStorageMock.Setup(mock => mock.DownloadImage(randomID)).ReturnsAsync((Image?)null);

        var result = await controller.DownloadImage(randomID);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFoundResult = (NotFoundObjectResult)result;
        Assert.Equal($"Image of id {randomID} not found.", notFoundResult.Value);
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("image/jpg")]
    [InlineData("image/jpeg")]
    public async Task DeleteImage_Valid(string validType)
    {
        string testId = "test-id";
        Image image = new(new MemoryStream(), validType);

        blobStorageMock.Setup(mock => mock.DownloadImage(testId)).ReturnsAsync(image);
        blobStorageMock.Setup(mock => mock.DeleteImage(testId)).Returns(Task.CompletedTask);

        var result = await controller.DeleteImage(testId);

        Assert.IsType<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.Equal($"Image of id {testId} successfully deleted", okResult.Value);
    }

    [Fact]
    public async Task DeleteImage_NotFound()
    {
        string testId = "test-id";

        blobStorageMock.Setup(mock => mock.DownloadImage(testId)).ReturnsAsync((Image?) null);

        var result = await controller.DeleteImage(testId);

        Assert.IsType<NotFoundObjectResult>(result);
        var notFoundResult = (NotFoundObjectResult)result;
        Assert.Equal($"Image of id {testId} not found", notFoundResult.Value);
    }

}