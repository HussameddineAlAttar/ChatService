using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Tests.ProfileTests;

public class CosmosProfileStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IProfileStore profileStore;

    private readonly Profile testProfile;
    private readonly string pictureID = Guid.NewGuid().ToString();
    private readonly string idForAlreadyExists = "conflict" + Guid.NewGuid().ToString();
    private readonly string idToDelete = "toDelete" + Guid.NewGuid().ToString();

    public CosmosProfileStoreTest(WebApplicationFactory<Program> factory)
    {
        profileStore = factory.Services.GetRequiredService<IProfileStore>();
        testProfile = new Profile("randomUsernameForTest", "FooTest", "BarTest", pictureID);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        try
        {
            await Task.WhenAll(profileStore.DeleteProfile(testProfile.Username), profileStore.DeleteProfile(idForAlreadyExists));
        }
        catch { }
    }

    [Fact]
    public async Task GetEmptyProfile()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await profileStore.GetProfile("");
        });
    }

    [Fact]
    public async Task GetNullProfile()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await profileStore.GetProfile(null);
        });
    }

    [Fact]
    public async Task AddProfile()
    {
        await profileStore.CreateProfile(testProfile);
        Assert.Equal(testProfile, await profileStore.GetProfile(testProfile.Username));
    }

    [Fact]
    public async Task AddProfile_AlreadyExists()
    {
        Profile alreadyExists = new Profile(idForAlreadyExists, idForAlreadyExists, idForAlreadyExists, idForAlreadyExists);
        await profileStore.CreateProfile(alreadyExists);

        await Assert.ThrowsAsync<ProfileConflictException>(async () =>
        {
            await profileStore.CreateProfile(alreadyExists);
        });
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
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await profileStore.CreateProfile(new Profile(username, username + "@email.com", firstname, lastname));
        });
    }

    [Fact]
    public async Task DeleteProfile()
    {
        Profile toDeleteProfile = new(idToDelete, idToDelete, idToDelete, idToDelete);
        await profileStore.CreateProfile(toDeleteProfile);
        await profileStore.DeleteProfile(toDeleteProfile.Username);

        await Assert.ThrowsAsync<ProfileNotFoundException>(async () =>
        {
            await profileStore.GetProfile(toDeleteProfile.Username);
        });
    }

    [Fact]
    public async Task DeleteEmptyProfile()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await profileStore.DeleteProfile("");
        });
    }

    [Fact]
    public async Task DeleteNullProfile()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await profileStore.DeleteProfile(null);
        });
    }

}
