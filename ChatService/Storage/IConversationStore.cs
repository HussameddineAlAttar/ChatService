using ChatService.DTO;

namespace ChatService.Storage;

public interface IConversationStore
{
    Task CreateConversation(Conversation conversation);
    Task<Conversation> FindConversationById(string conversationId, string username);
    Task<(List<Conversation> conversations, string? continuationToken)> EnumerateConversations(
                string username, int limit, long? lastSeenConversationTime, string continuationToken);
    Task UpdateLastModifiedTime(string conversationId, List<string> usernames, long unixTime);
    Task DeleteConversation(string conversationId, List<string> usernames);
}