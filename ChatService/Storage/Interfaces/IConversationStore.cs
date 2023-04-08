using ChatService.DTO;

namespace ChatService.Storage.Interfaces;

public interface IConversationStore
{
    Task CreateConversation(Conversation conversation, string username);
    Task<Conversation> FindConversationById(string id);
    Task<List<Conversation>> EnumerateConversations(string username);
    Task<(List<Conversation> conversations, string continuationToken)> EnumerateConversations(
                string username, int limit, long? lastSeenConversationTime, string continuationToken);
    Task ModifyTime(string username, string conversationId, long time);
    Task DeleteConversation(string conversationId, string username);
}
