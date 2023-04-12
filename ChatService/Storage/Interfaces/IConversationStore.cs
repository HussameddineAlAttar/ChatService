using ChatService.DTO;

namespace ChatService.Storage.Interfaces;

public interface IConversationStore
{
    Task CreateConversation(Conversation conversation, string username);
    Task<Conversation> FindConversationById(string id);
    Task<List<Conversation>> EnumerateConversations(string username);
    Task ModifyTime(string username, string conversationId, long time);
    Task DeleteConversation(string conversationId, string username);
}
