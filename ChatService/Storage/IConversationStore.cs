using ChatService.DTO;

namespace ChatService.Storage;

public interface IConversationStore
{
    Task CreateConversation(Conversation conversation);
    Task<Conversation> FindConversationById(string id);
    Task<(List<Conversation> conversations, string? continuationToken)> EnumerateConversations(
                string username, int limit, long? lastSeenConversationTime, string continuationToken);
    Task UpdateLastModifiedTime(string conversationId, long unixTime);
    Task DeleteConversation(string conversationId);
}