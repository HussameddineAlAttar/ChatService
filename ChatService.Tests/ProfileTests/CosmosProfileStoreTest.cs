using ChatService.DTO;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Tests.ProfileTests;

public class CosmosProfileStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IProfileInterface profileStore;

    private readonly Profile testProfile;
    private readonly string pictureID = Guid.NewGuid().ToString();

    public CosmosProfileStoreTest(WebApplicationFactory<Program> factory)
    {
        profileStore = factory.Services.GetRequiredService<IProfileInterface>();
        testProfile = new Profile("randomUsernameForTest", "FooTest", "BarTest", pictureID);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await profileStore.DeleteProfile(testProfile.Username);
    }

    [Fact]
    public async Task GetEmptyProfile() 
    {
        await Assert.ThrowsAsync<CosmosException>(async () =>
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
        await profileStore.UpsertProfile(testProfile);
        Assert.Equal(testProfile, await profileStore.GetProfile(testProfile.Username));
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
            await profileStore.UpsertProfile(new Profile(username, firstname, lastname, pictureID));
        });
    }


    [Fact]
    public async Task DeleteProfile()
    {
        string random = Guid.NewGuid().ToString();
        Profile toDeleteProfile = new(random, random, random, random);
        await profileStore.UpsertProfile(toDeleteProfile);
        Assert.Equal(toDeleteProfile, await profileStore.GetProfile(toDeleteProfile.Username));

        await profileStore.DeleteProfile(toDeleteProfile.Username);
        Assert.Null(await profileStore.GetProfile(toDeleteProfile.Username));
    }

    [Fact]
    public async Task DeleteEmptyProfile() 
    {
        await Assert.ThrowsAsync<CosmosException>(async () =>
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
