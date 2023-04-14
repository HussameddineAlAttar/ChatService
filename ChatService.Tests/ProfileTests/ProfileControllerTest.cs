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
using ChatService.Exceptions;
using ChatService.Storage;

namespace ChatService.Tests.ProfileTests;

public class ProfileControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IProfileStore> profileStoreMock = new();
    private readonly HttpClient httpClient;

    private readonly Profile testProfile;


    public ProfileControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(profileStoreMock.Object); });
        }).CreateClient();
        testProfile = new Profile("Test_FooBar", "FooTest", "BarTest", Guid.NewGuid().ToString());
    }

    [Fact]
    public async Task GetProfile()
    {
        profileStoreMock.Setup(m => m.GetProfile(testProfile.Username)).ReturnsAsync(testProfile);

        var response = await httpClient.GetAsync($"/api/profile/{testProfile.Username}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(testProfile, JsonConvert.DeserializeObject<Profile>(json));
    }

    [Fact]
    public async Task GetNonExistingProfile()
    {
        profileStoreMock.Setup(m => m.GetProfile(testProfile.Username)).ThrowsAsync(new ProfileNotFoundException());

        var response = await httpClient.GetAsync($"/api/profile/{testProfile.Username}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddProfile()
    {
        var response = await httpClient.PostAsync("/api/profile",
            new StringContent(JsonConvert.SerializeObject(testProfile), Encoding.Default, "application/json"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var uploadedProfile = JsonConvert.DeserializeObject<Profile>(json);
        profileStoreMock.Verify(mock => mock.CreateProfile(uploadedProfile), Times.Once());
    }

    [Fact]
    public async Task AddProfile_AlreadyExists()
    {
        profileStoreMock.Setup(m => m.CreateProfile(testProfile)).ThrowsAsync(new ProfileConflictException());

        var response = await httpClient.PostAsync("/api/profile",
            new StringContent(JsonConvert.SerializeObject(testProfile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        profileStoreMock.Verify(mock => mock.CreateProfile(testProfile), Times.Once);
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
        Profile profile = new(username, firstname, lastname, Guid.NewGuid().ToString());
        var response = await httpClient.PostAsync("/api/profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        profileStoreMock.Verify(mock => mock.CreateProfile(profile), Times.Never);

    }
}