using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage.Entities;
using ChatService.Storage.Interfaces;
using Microsoft.Azure.Cosmos;
using ChatService.Services;
using System.Net;
using System.Text;
using System;

namespace ChatService.Storage.Implementations;

public class CosmosConversationStore : IConversationStore
{
    private readonly CosmosClient cosmosClient;

    public CosmosConversationStore(CosmosClient cosmosClient)
    {
        this.cosmosClient = cosmosClient;
    }

    private Container Container => cosmosClient.GetDatabase("Profiles").GetContainer("Conversations");

    public async Task CreateConversation(Conversation conversation, string username)
    {
        try
        {
            await Container.CreateItemAsync(ToEntity(conversation, username));
        }
        catch(CosmosException e)
        {
            if(e.StatusCode == HttpStatusCode.Conflict)
            {
                throw new ConversationConflictException();
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
                partitionKey: new PartitionKey(conversationId.Split("_")[0]),
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
                throw new ConversationNotFoundException();
            }
            throw;
        }
    }

    public async Task<List<Conversation>> EnumerateConversations(string username)
    {
        var query = new QueryDefinition("SELECT * FROM Conversations c WHERE c.partitionKey = @partitionKey").WithParameter("@partitionKey", username);
        var iterator = Container.GetItemQueryIterator<ConversationEntity>(query);
        List<Conversation> conversations = new();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();

            foreach (var entity in response)
            {
                conversations.Add(ToConversation(entity));
            }
        }

        if(conversations.Count == 0)
        {
            throw new ConversationNotFoundException();
        }

        conversations = conversations.OrderByDescending(x => x.ModifiedTime).ToList();
        return conversations;
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
            ConsistencyLevel = ConsistencyLevel.Session
        };
        var iterator = Container.GetItemQueryIterator<ConversationEntity>(query, requestOptions:queryOptions, continuationToken: continuationToken);
        var response = await iterator.ReadNextAsync();

        List<Conversation> conversations = new();

        foreach (var entity in response)
        {
            conversations.Add(ToConversation(entity));
        }

        if (conversations.Count == 0)
        {
            throw new ConversationNotFoundException();
        }

        return (conversations, response.ContinuationToken);
    }


    public async Task ModifyTime(string username, string conversationId, long time)
    {
        try
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
            conversation.ModifiedTime = time;
            await Container.UpsertItemAsync(ToEntity(conversation, username));  
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ConversationNotFoundException();
            }
            throw;
        }
    }

    public async Task DeleteConversation(string conversationId, string username)
    {
        try
        {
            await Container.DeleteItemAsync<Message>(
                id: conversationId,
                partitionKey: new PartitionKey(username)
            );
        }
        catch
        {
            return;
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
