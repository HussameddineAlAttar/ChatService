using ChatService.DTO;

namespace ChatService.Storage.Interfaces;

public interface IMessagesStore
{
    Task SendMessage(string conversationId, Message message);
    Task<List<Message>> EnumerateMessages(string conversationId);
    Task DeleteMessage(string conversationId, string messageId);
    Task<Message> GetMessageById(string conversationId, string messageId);
}
