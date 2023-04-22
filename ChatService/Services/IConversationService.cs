using ChatService.DTO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ChatService.Services;

public interface IConversationService
{
    Task CreateConversation(CreateConvoRequest conversationRequest);
    Task<long> UpdateLastModifiedTime(string conversationId, long unixTime);
    Task<ConvoResponseWithToken> EnumerateConversations(string username, int limit = 10, long? lastSeenConversationTime = null, string? continuationToken = null);
}