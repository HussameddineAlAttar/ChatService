namespace ChatService.Storage.Entities;

public record ProfileEntity(string partitionKey, string id, string email, string password, string firstName, string lastName);