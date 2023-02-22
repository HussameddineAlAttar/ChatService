using System.Net;
using ChatService.DTO;
using Microsoft.Azure.Cosmos;
using ChatService.Storage.Entities;
using ChatService.Storage;

namespace ChatService.Storage;

public class CosmosProfileStore : IProfileInterface
{
    private readonly CosmosClient _cosmosClient;

    public CosmosProfileStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("Profiles").GetContainer("profiles");

    public async Task UpsertProfile(Profile profile)
    {
        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.userName) ||
            string.IsNullOrWhiteSpace(profile.firstName) ||
            string.IsNullOrWhiteSpace(profile.lastName)
           )
        {
            throw new ArgumentException($"Invalid profile {profile}", nameof(profile));
        }
        await Container.UpsertItemAsync(ToEntity(profile));
    }

    public async Task<Profile?> GetProfile(string username)
    {
        try
        {
            var entity = await Container.ReadItemAsync<ProfileEntity>(
                id: username,
                partitionKey: new PartitionKey(username),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            return ToProfile(entity);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            throw;
        }
    }

    public async Task DeleteProfile(string username)
    {
        try
        {
            await Container.DeleteItemAsync<Profile>(
                id: username,
                partitionKey: new PartitionKey(username)
            );
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }

            throw;
        }
    }

    private static ProfileEntity ToEntity(Profile profile)
    {
        return new ProfileEntity(
            partitionKey: profile.userName,
            id: profile.userName,
            profile.firstName,
            profile.lastName,
            profile.ProfilePictureID
        ); ;
    }

    private static Profile ToProfile(ProfileEntity entity)
    {
        Profile toReturn = new(entity.id, entity.firstName, entity.lastName);
        toReturn.ProfilePictureID = entity.profilePictureID;
        return toReturn;
    }
}