using System.Net;
using ChatService.DTO;
using Microsoft.Azure.Cosmos;
using ChatService.Storage.Entities;
using ChatService.Exceptions;
using ChatService.Storage.Interfaces;

namespace ChatService.Storage.Implementations;

public class CosmosProfileStore : IProfileInterface
{
    private readonly CosmosClient _cosmosClient;

    public CosmosProfileStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("Profiles").GetContainer("profiles");

    public async Task CreateProfile(Profile profile)
    {
        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.Username) ||
            string.IsNullOrWhiteSpace(profile.FirstName) ||
            string.IsNullOrWhiteSpace(profile.LastName)
           )
        {
            throw new ArgumentException($"Invalid profile {profile}", nameof(profile));
        }
        try
        {
            await Container.CreateItemAsync(ToEntity(profile));
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.Conflict)
            {
                throw new ProfileConflictException();
            }
            throw;
        }
    }

    public async Task<Profile> GetProfile(string username)
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
                throw new ProfileNotFoundException();
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
                throw new ProfileNotFoundException();
            }
            throw;
        }
    }

    private static ProfileEntity ToEntity(Profile profile)
    {
        return new ProfileEntity(
            partitionKey: profile.Username,
            id: profile.Username,
            profile.FirstName,
            profile.LastName,
            profile.ProfilePictureId
        ); ;
    }

    private static Profile ToProfile(ProfileEntity entity)
    {
        Profile toReturn = new(entity.id, entity.firstName, entity.lastName, entity.profilePictureID);
        toReturn.ProfilePictureId = entity.profilePictureID;
        return toReturn;
    }

}