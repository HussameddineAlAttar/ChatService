using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage.Entities;
using ChatService.Storage;
using Microsoft.Azure.Cosmos;
using ChatService.Services;
using System.Net;
using System.Text;
using System;
using ChatService.Extensions;
using Microsoft.VisualBasic;
using Microsoft.Azure.Storage;

namespace ChatService.Storage.Cosmos;

public class CosmosConversationStore : IConversationStore
{
    private readonly CosmosClient cosmosClient;

    public CosmosConversationStore(CosmosClient cosmosClient)
    {
        this.cosmosClient = cosmosClient;
    }

    private Container Container => cosmosClient.GetDatabase("Profiles").GetContainer("Conversations");

    public async Task CreateConversation(Conversation conversation)
    {
        try
        {
            var tasks = conversation.Participants.Select(async username =>
            {
                await Container.CreateItemAsync(ToEntity(conversation, username));
            });
            await Task.WhenAll(tasks);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.Conflict)
            {
                throw new ConversationConflictException($"Conversation {conversation.Id} already exists.");
            }
            throw;
        }
    }

    public async Task<Conversation> FindConversationById(string conversationId)
    {
        try
        {
            var entity = await Container.ReadItemAsync<ConversationEntity>(
                id: conversationId,
                partitionKey: new PartitionKey(conversationId.SplitToUsernames()[0]),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            return ToConversation(entity);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ConversationNotFoundException($"Conversation with id {conversationId} not found.");
            }
            throw;
        }
    }

    public async Task<(List<Conversation> conversations, string continuationToken)> EnumerateConversations(
                string username, int limit, long? lastSeenConversationTime, string continuationToken)
    {
        string queryString = "SELECT * FROM Conversations c WHERE c.partitionKey = @partitionKey" +
                                " AND c.lastModifiedTime >= @lastSeenConversationTime" +
                                " ORDER BY c.lastModifiedTime DESC";

        var query = new QueryDefinition(queryString)
            .WithParameter("@partitionKey", username)
            .WithParameter("@lastSeenConversationTime", lastSeenConversationTime);

        var queryOptions = new QueryRequestOptions
        {
            MaxItemCount = limit,
            ConsistencyLevel = ConsistencyLevel.Session,
        };
        var iterator = Container.GetItemQueryIterator<ConversationEntity>(query, requestOptions: queryOptions, continuationToken: WebUtility.UrlDecode(continuationToken));
        var response = await iterator.ReadNextAsync();

        List<Conversation> conversations = new();

        foreach (var entity in response)
        {
            conversations.Add(ToConversation(entity));
        }

        if (conversations.Count == 0)
        {
            throw new ConversationNotFoundException($"No more conversations found for user {username}.");
        }

        return (conversations, response.ContinuationToken);
    }


    public async Task UpdateLastModifiedTime(string conversationId, long unixTime)
    {
        List<string> usernames = conversationId.SplitToUsernames();
        try
        {
            var tasks = usernames.Select(async username =>
            {
                var entity = await Container.ReadItemAsync<ConversationEntity>(
                    id: conversationId,
                    partitionKey: new PartitionKey(username),
                    new ItemRequestOptions
                    {
                        ConsistencyLevel = ConsistencyLevel.Session
                    }
                );
                var conversation = ToConversation(entity);
                conversation.ModifiedTime = unixTime;
                await Container.UpsertItemAsync(ToEntity(conversation, username));
            });
            await Task.WhenAll(tasks);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ConversationNotFoundException($"Conversation with id {conversationId} not found.");
            }
            throw;
        }
    }


    public async Task DeleteConversation(string conversationId)
    {
        List<string> usernames = conversationId.SplitToUsernames();
        try
        {
            var tasks = usernames.Select(async username =>
            {
                await Container.DeleteItemAsync<Message>(
                id: conversationId,
                partitionKey: new PartitionKey(username)
                );
            });
            await Task.WhenAll(tasks);
        }
        catch(CosmosException e) 
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
            throw;
        }
    }

    public static ConversationEntity ToEntity(Conversation conversation, string username)
    {
        return new ConversationEntity(
            partitionKey: username,
            id: conversation.Id,
            participants: conversation.Participants,
            lastModifiedTime: conversation.ModifiedTime,
            createdTime: conversation.CreatedTime
            );
    }

    public static Conversation ToConversation(ConversationEntity entity)
    {
        Conversation toReturn = new(entity.participants);
        toReturn.ModifiedTime = entity.lastModifiedTime;
        toReturn.CreatedTime = entity.createdTime;
        toReturn.Id = entity.id;
        return toReturn;
    }
}
