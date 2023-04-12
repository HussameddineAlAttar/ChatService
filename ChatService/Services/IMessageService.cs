using ChatService.DTO;

namespace ChatService.Services;

public interface IMessageService
{
    Task<long> SendMessage(string conversationId, Message message, bool FirstTime = false);
    Task<List<EnumMessageResponse>> EnumerateMessages(string conversationId);
}
