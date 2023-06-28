using System.Net;
using ChatService.DTO;
using Microsoft.Azure.Cosmos;
using ChatService.Storage.Entities;
using ChatService.Exceptions;

namespace ChatService.Storage.Cosmos;

public class CosmosProfileStore : IProfileStore
{
    private readonly CosmosClient _cosmosClient;

    public CosmosProfileStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("Profiles").GetContainer("profiles");

    public async Task CreateProfile(Profile profile)
    {
        try
        {
            await Container.CreateItemAsync(ToEntity(profile));
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.Conflict)
            {
                throw new ProfileConflictException($"Username {profile.Username} is taken.");
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
                throw new ProfileNotFoundException($"Profile of username {username} not found.");
            }
            throw;
        }
    }

    public async Task<Profile> GetProfileByEmail(string email)
    {
        var query = new QueryDefinition("SELECT TOP 1 * FROM p WHERE p.email = @EMAIL")
            .WithParameter("@EMAIL", email);

        var iterator = Container.GetItemQueryIterator<ProfileEntity>(query);

        var results = await iterator.ReadNextAsync();

        if (results.Count == 0)
        {
            throw new ProfileNotFoundException($"Profile with email {email} not found.");
        }
        return ToProfile(results.FirstOrDefault());
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
                throw new ProfileNotFoundException($"Profile of username {username} not found.");
            }
            throw;
        }
    }

    private static ProfileEntity ToEntity(Profile profile)
    {
        return new ProfileEntity(
            partitionKey: profile.Username,
            id: profile.Username,
            profile.Email,
            profile.Password,
            profile.FirstName,
            profile.LastName
        );
    }

    private static Profile ToProfile(ProfileEntity entity)
    {
        Profile toReturn = new(entity.id, entity.email, entity.password, entity.firstName, entity.lastName);
        return toReturn;
    }
}