﻿using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage.Entities;
using ChatService.Storage.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Net;
namespace ChatService.Storage.Implementations;

public class CosmosMessageStore : IMessagesStore
{
    private readonly CosmosClient cosmosClient;

    public CosmosMessageStore(CosmosClient cosmosClient)
    {
        this.cosmosClient = cosmosClient;
    }

    private Container Container => cosmosClient.GetDatabase("Profiles").GetContainer("Messages");

    public async Task SendMessage(string conversationId, Message message)
    {
        try
        {
            await Container.CreateItemAsync(ToEntity(message, conversationId));
        }
        catch(CosmosException e)
        {
            if(e.StatusCode == HttpStatusCode.Conflict)
            {
                throw new MessageConflictException();
            }
            throw;
        }
    }

    public async Task<List<Message>> EnumerateMessages(string conversationId)
    {
        var entities = new List<MessageEntity>();
        var iterator = Container.GetItemLinqQueryable<MessageEntity>()
                                 .Where(x => x.partitionKey == conversationId)
                                 .ToFeedIterator();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            entities.AddRange(response);
        }

        if (entities.Count == 0)
        {
            throw new ConversationNotFoundException();
        }

        var messages = entities.Select(ToMessage).OrderByDescending(x => x.Time).ToList();
        return messages;
    }

    public async Task DeleteMessage(string conversationId, string messageId)
    {
        try
        {
            await Container.DeleteItemAsync<Message>(
                id: messageId,
                partitionKey: new PartitionKey(conversationId)
            );
        }
        catch
        {
            return;
        }
    }

    public async Task<Message> GetMessageById(string conversationId, string messageId)
    {
        try
        {
            var entity = await Container.ReadItemAsync<MessageEntity>(
                id: messageId,
                partitionKey: new PartitionKey(conversationId),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            return ToMessage(entity);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                throw new MessageNotFoundException();
            }
            throw;
        }
    }

    public static MessageEntity ToEntity(Message message, string conversationId)
    {
        return new MessageEntity(
            partitionKey: conversationId,
            id: message.Id,
            text: message.Text,
            senderUsername: message.SenderUsername,
            createdTime: message.Time
            );
    }

    public static Message ToMessage(MessageEntity entity)
    {
        Message toReturn = new(entity.senderUsername, entity.text, entity.id, entity.createdTime);
        return toReturn;
    }

}
