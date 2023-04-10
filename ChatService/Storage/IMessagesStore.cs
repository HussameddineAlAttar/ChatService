using ChatService.DTO;

namespace ChatService.Storage;

public interface IMessagesStore
{
    Task SendMessage(string conversationId, Message message);
    Task<List<Message>> EnumerateMessages(string conversationId);
    Task<(List<Message> messages, string continuationToken)> EnumerateMessages(
                string conversationId, int limit, long? lastSeenMessageTime, string continuationToken);
    Task DeleteMessage(string conversationId, string messageId);
    Task<Message> GetMessageById(string conversationId, string messageId);
}
