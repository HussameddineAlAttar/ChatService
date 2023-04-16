using ChatService.DTO;

namespace ChatService.Services;

public interface IConversationService
{
    Task CreateConversation(CreateConvoRequest conversationRequest);
    Task<List<ConversationResponse>> EnumerateConversations(string username);
    Task<long> UpdateLastModifiedTime(string conversationId, long unixTime);
    Task<(List<ConversationResponse> conversations, string token)> GetConversations(string username, int limit = 10, long? lastSeenConversationTime = null, string? continuationToken = null);
}