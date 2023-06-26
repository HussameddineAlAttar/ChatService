using ChatService.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Moq;
using System.Net;
using Newtonsoft.Json;
using ChatService.Exceptions;
using ChatService.Storage;
using ChatService.Services;
using Microsoft.AspNetCore.Http;
using ChatService.Extensions;

namespace ChatService.Tests.ImageTests;

public class ImageControllerTest: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IImageService> imageService = new();
    private readonly HttpClient httpClient;

    private readonly MultipartFormDataContent dataContent = new();
    private readonly string testID = Guid.NewGuid().ToString();

    public ImageControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(imageService.Object); });
        }).CreateClient();
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("image/jpeg")]
    public async Task UploadValidImage(string fileType)
    {
        Stream imageStream = new MemoryStream();
        var testUploadImageResponse = new UploadImageResponse(testID);

        var fileToUpload = new StreamContent(imageStream);
        dataContent.Add(fileToUpload, "File", "image");
        dataContent.Add(new StringContent("username"), testID);
        fileToUpload.Headers.ContentType = new MediaTypeHeaderValue(fileType);

        var clientResponse = await httpClient.PostAsync($"/api/images/{testID}", dataContent);
        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var json = await clientResponse.Content.ReadAsStringAsync();
        UploadImageResponse receivedResponse = JsonConvert.DeserializeObject<UploadImageResponse>(json);
        Assert.Equal(testUploadImageResponse, receivedResponse);
    }

    [Fact]
    public async Task DownloadValidImage()
    {
        var expectedContent = new byte[] { 0x12, 0x34, 0x56, 0x78 };
        imageService.Setup(m => m.DownloadImage(testID.HashSHA256())).ReturnsAsync(expectedContent);

        var response = await httpClient.GetAsync($"/api/images/{testID}");
        var responseContent = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(expectedContent, responseContent);
    }


    [Fact]
    public async Task DownloadImage_NotFound()
    {
        string randomID = "randomID_doesn't_exist";
        imageService.Setup(mock => mock.DownloadImage(randomID.HashSHA256())).ThrowsAsync(new ImageNotFoundException());

        var result = await httpClient.GetAsync($"/api/images/{randomID}");
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
}