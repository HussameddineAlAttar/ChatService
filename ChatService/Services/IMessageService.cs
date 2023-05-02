using ChatService.DTO;
using Newtonsoft.Json.Linq;

namespace ChatService.Services;

public interface IMessageService
{
    Task<long> SendMessage(string conversationId, Message message);
    Task<(List<EnumMessageResponse>, string token)> EnumerateMessages(string conversationId, int limit = 10, long lastSeenMessageTime = 1, string? continuationToken = null);
}
