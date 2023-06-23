using ChatService.DTO;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Microsoft.AspNetCore.TestHost;
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

    private bool EqualProfile(Profile p1, Profile p2)
    {
        return (p1.Username == p2.Username) && (p1.Email == p2.Email) &&
            (p1.FirstName == p2.FirstName) && (p1.LastName == p2.LastName);
    }

    public ProfileControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(profileStoreMock.Object); });
        }).CreateClient();
        testProfile = new Profile("Test_FooBar", "Test_FooBar@email.com", Guid.NewGuid().ToString(), "FooTest", "BarTest");
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
        profileStoreMock.Setup(m => m.GetProfileByEmail(testProfile.Email)).ThrowsAsync(new ProfileNotFoundException());
        var response = await httpClient.PostAsync("/api/profile",
            new StringContent(JsonConvert.SerializeObject(testProfile), Encoding.Default, "application/json"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        var uploadedProfile = JsonConvert.DeserializeObject<Profile>(json);
        profileStoreMock.Verify(mock => mock.CreateProfile(uploadedProfile), Times.Once());
    }

    [Fact]
    public async Task AddProfile_EmailTaken()
    {
        profileStoreMock.Setup(m => m.GetProfileByEmail(testProfile.Email)).ReturnsAsync(testProfile);
        string idk = testProfile.Email;

        var response = await httpClient.PostAsync("/api/profile",
            new StringContent(JsonConvert.SerializeObject(testProfile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        profileStoreMock.Verify(mock => mock.CreateProfile(testProfile), Times.Never);
    }

    [Fact]
    public async Task AddProfile_UsernameTaken()
    {
        profileStoreMock.Setup(m => m.GetProfileByEmail(testProfile.Email)).ThrowsAsync(new ProfileNotFoundException());

        profileStoreMock.Setup(m => m.CreateProfile(It.Is<Profile>(pf => EqualProfile(pf, testProfile)))).ThrowsAsync(new ProfileConflictException());

        var response = await httpClient.PostAsync("/api/profile",
            new StringContent(JsonConvert.SerializeObject(testProfile), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        profileStoreMock.Verify(mock => mock.CreateProfile(It.Is<Profile>(pf => EqualProfile(pf, testProfile))), Times.Once);
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
        Profile profile = new(username, username + "@email.com", Guid.NewGuid().ToString(), firstname, lastname);
        var response = await httpClient.PostAsync("/api/profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        profileStoreMock.Verify(mock => mock.CreateProfile(profile), Times.Never);

    }
}