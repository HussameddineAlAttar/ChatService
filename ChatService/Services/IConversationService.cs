using ChatService.DTO;

namespace ChatService.Services;

public interface IConversationService
{
    Task CreateConversation(CreateConvoRequest conversationRequest);
    Task<List<ConversationResponse>> EnumerateConversations(string username);
    Task<long> ModifyTime(string conversationId, long time);
}
