using ChatService.DTO;
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
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Azure.Cosmos;
using ChatService.Exceptions;
using ChatService.Storage.Interfaces;

namespace ChatService.Tests.ImageTests;

public class ImageControllerTest: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IProfileInterface> profileStoreMock = new();
    private readonly Mock<IImageInterface> blobStorageMock = new();
    private readonly HttpClient httpClient;

    private readonly MultipartFormDataContent dataContent = new();
    private readonly string testID = Guid.NewGuid().ToString();

    public ImageControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(profileStoreMock.Object); });
            builder.ConfigureTestServices(services => { services.AddSingleton(blobStorageMock.Object); });
        }).CreateClient();
    }

    [Theory]
    [InlineData("png")]
    [InlineData("jpeg")]
    [InlineData("jpg")]
    public async Task UploadValidImage(string validType)
    {
        Image testImage = new(new MemoryStream(), "image/" + validType);

        var testUploadImageResponse = new UploadImageResponse(testID);

        blobStorageMock.Setup(m => m.UploadImage(It.IsAny<UploadImageRequest>())).ReturnsAsync(testID);

        var fileToUpload = new StreamContent(testImage.Content);
        fileToUpload.Headers.ContentType = new MediaTypeHeaderValue(testImage.ContentType);

        dataContent.Add(fileToUpload, "File", "image." + validType);

        var clientResponse = await httpClient.PostAsync("/images", dataContent);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var json = await clientResponse.Content.ReadAsStringAsync();
        UploadImageResponse receivedResponse = JsonConvert.DeserializeObject<UploadImageResponse>(json);
        Assert.Equal(testUploadImageResponse, receivedResponse);

    }

    [Theory]
    [InlineData("image", "gif")]
    [InlineData("application", "pdf")]
    [InlineData("text", "plain")]
    [InlineData("video", "mp4")]
    public async Task UploadNonImage_Bad(string invalidType, string extension)
    {
        Image testImage = new(new MemoryStream(), $"{invalidType}/{extension}");

        //var request = new UploadImageRequest(fileMock.Object);
        var testUploadImageResponse = new UploadImageResponse(testID);

        blobStorageMock.Setup(m => m.UploadImage(It.IsAny<UploadImageRequest>())).ReturnsAsync(testID);

        var fileToUpload = new StreamContent(testImage.Content);
        fileToUpload.Headers.ContentType = new MediaTypeHeaderValue(testImage.ContentType);

        dataContent.Add(fileToUpload, "File", $"{invalidType}.{extension}");

        var clientResponse = await httpClient.PostAsync("/images", dataContent);

        Assert.Equal(HttpStatusCode.BadRequest, clientResponse.StatusCode);
    }


    [Theory]
    [InlineData("png")]
    [InlineData("jpeg")]
    [InlineData("jpg")]
    public async Task DownloadValidImage(string type)
    {
        // Arrange
        var expectedContent = new byte[] { 0x12, 0x34, 0x56, 0x78 };
        var image = new Image(new MemoryStream(expectedContent), "image/" + type);
        blobStorageMock.Setup(m => m.DownloadImage(testID)).ReturnsAsync(image);


        // Act
        var response = await httpClient.GetAsync($"/images/{testID}");

        // Assert
        var contentType = response.Content.Headers.ContentType.ToString();
        Assert.Equal(image.ContentType, contentType);

        var responseContent = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(expectedContent, responseContent);
    }


    [Fact]
    public async Task DownloadImage_NotFound()
    {
        string randomID = "randomID_doesn't_exist";
        blobStorageMock.Setup(mock => mock.DownloadImage(randomID)).ThrowsAsync(new ImageNotFoundException());

        var result = await httpClient.GetAsync($"/images/{randomID}");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

}