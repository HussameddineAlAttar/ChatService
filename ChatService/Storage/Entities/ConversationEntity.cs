namespace ChatService.Storage.Entities;

public record ConversationEntity(string partitionKey, string id, List<string> participants, long lastModifiedTime, long createdTime);