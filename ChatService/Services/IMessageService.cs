using ChatService.DTO;

namespace ChatService.Services;

public interface IMessageService
{
    Task<long> SendMessage(string conversationId, Message message, bool FirstTime = false);
    Task<MessageTokenResponse> EnumerateMessages(string conversationId, int limit = 10, long lastSeenMessageTime = 1, string? continuationToken = null);
}
