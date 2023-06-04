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
        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.Username) ||
            string.IsNullOrWhiteSpace(profile.Email) ||
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
                throw new ProfileConflictException($"Username {profile.Username} is taken.");
            }
            throw;
        }
    }

    public async Task<Profile> GetProfile(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException($"Username cannot be empty");
        }
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
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentNullException($"Username cannot be empty");
        }
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
            profile.FirstName,
            profile.LastName,
            profile.ProfilePictureId
        );
    }

    private static Profile ToProfile(ProfileEntity entity)
    {
        Profile toReturn = new(entity.id, entity.email, entity.firstName, entity.lastName, entity.profilePictureID);
        return toReturn;
    }
}