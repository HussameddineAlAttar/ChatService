namespace ChatService.Storage.Entities;

public record MessageEntity(string partitionKey, string id, string senderUsername, string text, long createdTime);