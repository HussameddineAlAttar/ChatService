using ChatService.Storage;
using ChatService.DTO;
using ChatService.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Newtonsoft.Json;
using System.Text;

namespace ChatService.Tests.ProfileTests;

public class ProfileControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IProfileInterface> profileStoreMock = new();
    private readonly Mock<IImageInterface> blobStorageMock = new();
    private readonly HttpClient httpClient;

    private readonly Profile testProfile;
    private readonly IncompleteProfile incProfile;

    public ProfileControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(profileStoreMock.Object); });
            builder.ConfigureTestServices(services => { services.AddSingleton(blobStorageMock.Object); });
        }).CreateClient();
        incProfile = new IncompleteProfile("Test_FooBar", "FooTest", "BarTest");
        testProfile = new Profile(incProfile.userName, incProfile.firstName, incProfile.lastName);
    }

    [Fact]
    public async Task GetProfile()
    {
        profileStoreMock.Setup(m => m.GetProfile(testProfile.userName)).ReturnsAsync(testProfile);

        var response = await httpClient.GetAsync($"/profile/{testProfile.userName}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(testProfile, JsonConvert.DeserializeObject<Profile>(json));
    }

    [Fact]
    public async Task GetNonExistingProfile()
    {
        profileStoreMock.Setup(m => m.GetProfile(testProfile.userName)).ReturnsAsync((Profile?)null);

        var response = await httpClient.GetAsync($"/profile/{testProfile.userName}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddProfile()
    {
        var response = await httpClient.PostAsync("/profile",
            new StringContent(JsonConvert.SerializeObject(incProfile), Encoding.Default, "application/json"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"http://localhost/profile/{incProfile.userName}", response.Headers.GetValues("Location").First());

        var json = await response.Content.ReadAsStringAsync();
        var uploadedProfile = JsonConvert.DeserializeObject<Profile>(json);
        profileStoreMock.Verify(mock => mock.UpsertProfile(uploadedProfile), Times.Once());
    }

    [Fact]
    public async Task AddProfile_AlreadyExists()
    {
        profileStoreMock.Setup(m => m.GetProfile(testProfile.userName)).ReturnsAsync(testProfile);

        var response = await httpClient.PostAsync("/profile",
            new StringContent(JsonConvert.SerializeObject(testProfile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        profileStoreMock.Verify(mock => mock.UpsertProfile(testProfile), Times.Never);
    }

    [Theory]
    [InlineData(null, "Foo", "Bar")]
    [InlineData("", "Foo", "Bar")]
    [InlineData(" ", "Foo", "Bar")]
    [InlineData("foobar", null, "Bar")]
    [InlineData("foobar", "", "Bar")]
    [InlineData("foobar", "   ", "Bar")]
    [InlineData("foobar", "Foo", "")]
    [InlineData("foobar", "Foo", null)]
    [InlineData("foobar", "Foo", " ")]
    public async Task AddProfile_InvalidArgs(string username, string firstname, string lastname)
    {
        Profile profile = new(username, firstname, lastname);
        var response = await httpClient.PostAsync("/profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        profileStoreMock.Verify(mock => mock.UpsertProfile(profile), Times.Never);

    }

    [Fact]
    public async Task DeleteProfile()
    {
        profileStoreMock.Setup(m => m.GetProfile(testProfile.userName)).ReturnsAsync(testProfile);

        var response = await httpClient.DeleteAsync($"/profile/{testProfile.userName}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        profileStoreMock.Verify(mock => mock.DeleteProfile(testProfile.userName), Times.Once);
    }

    [Fact]
    public async Task DeleteProfile_NotExisting()
    {
        profileStoreMock.Setup(m => m.GetProfile(testProfile.userName)).ReturnsAsync((Profile?)null);
        var response = await httpClient.DeleteAsync($"/profile/{testProfile.userName}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        profileStoreMock.Verify(mock => mock.DeleteProfile(testProfile.userName), Times.Never);
    }
}