using ChatService.DTO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ChatService.Services;

public interface IConversationService
{
    Task CreateConversation(CreateConversationRequest conversationRequest);
    Task<long> UpdateLastModifiedTime(string conversationId, long unixTime);
    Task<(List<EnumerateConversationsEntry>, string token)> EnumerateConversations(string username, int limit = 10, long? lastSeenConversationTime = null, string? continuationToken = null);
}