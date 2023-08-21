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
    private readonly Profile emailProfile;

    private readonly string idForAlreadyExists = "conflict" + Guid.NewGuid().ToString();
    private readonly string idToDelete = "toDelete" + Guid.NewGuid().ToString();
    public readonly string idForEmail = "email" + Guid.NewGuid().ToString();

    public CosmosProfileStoreTest(WebApplicationFactory<Program> factory)
    {
        profileStore = factory.Services.GetRequiredService<IProfileStore>();
        testProfile = new Profile("randomUsernameForTest", "Foo@email.com", Guid.NewGuid().ToString(), "FooTest", "BarTest");
        emailProfile = new Profile("RandomUsernameForEmail", "FooMail@email.com", Guid.NewGuid().ToString(), "FooTest", "BarTest");
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        try
        {
            await Task.WhenAll(
                profileStore.DeleteProfile(testProfile.Username),
                profileStore.DeleteProfile(idForAlreadyExists),
                profileStore.DeleteProfile(emailProfile.Username)
                );
        }
        catch { }
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
        Profile alreadyExists = new(idForAlreadyExists, idForAlreadyExists + "@email.com", idForAlreadyExists, idForAlreadyExists, idForAlreadyExists);
        await profileStore.CreateProfile(alreadyExists);

        await Assert.ThrowsAsync<ProfileConflictException>(async () =>
        {
            await profileStore.CreateProfile(alreadyExists);
        });
    }

    [Fact]
    public async Task GetProfileByEmail()
    {
        await profileStore.CreateProfile(emailProfile);
        Assert.Equal(emailProfile, await profileStore.GetProfileByEmail(emailProfile.Email));
    }

    [Fact]
    public async Task GetProfileByEmail_NotExisting()
    {
        await Assert.ThrowsAsync<ProfileNotFoundException>(async () =>
        {
            await profileStore.GetProfileByEmail(Guid.NewGuid().ToString());
        });
    }

    [Fact]
    public async Task DeleteProfile()
    {
        Profile toDeleteProfile = new(idToDelete, idToDelete, Guid.NewGuid().ToString(), idToDelete, idToDelete);
        await profileStore.CreateProfile(toDeleteProfile);
        await profileStore.DeleteProfile(toDeleteProfile.Username);

        await Assert.ThrowsAsync<ProfileNotFoundException>(async () =>
        {
            await profileStore.GetProfile(toDeleteProfile.Username);
        });

        await Assert.ThrowsAsync<ProfileNotFoundException>(async () =>
        {
            await profileStore.DeleteProfile(toDeleteProfile.Username);
        });
    }
}
