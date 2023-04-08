using ChatService.DTO;

namespace ChatService.Services;

public interface IMessageService
{
    Task<long> SendMessage(string conversationId, Message message, bool FirstTime = false);
    Task<List<EnumMessageResponse>> EnumerateMessages(string conversationId);
    Task<MessageTokenResponse> GetMessages(string conversationId, int limit = 10, long? lastSeenMessageTime = null, string? continuationToken = null);
}
