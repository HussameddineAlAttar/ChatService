namespace ChatService.Storage.Entities;

public record ProfileEntity(string partitionKey, string id, string email, string firstName, string lastName, string profilePictureID);