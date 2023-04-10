using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Extensions;
using ChatService.Storage;
using System.Net;

namespace ChatService.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationStore conversationStore;
    private readonly IMessageService messageService;
    private readonly IProfileStore profileStore;

    public ConversationService(IConversationStore _conversationStore, IProfileStore _profileStore, IMessageService _messageService)
    {
        conversationStore = _conversationStore;
        profileStore = _profileStore;
        messageService = _messageService;
    }

    public async Task CreateConversation(CreateConvoRequest convoRequest)
    {
        var conversation = convoRequest.Conversation;
        var NonExistingProfiles = await profileStore.CheckForNonExistingProfile(conversation.Participants);
        if(NonExistingProfiles.Count > 0)
        {
            throw new ProfileNotFoundException(NonExistingProfiles);
        }
        try
        {
            await messageService.SendMessage(conversation.Id, convoRequest.FirstMessageRequest.message, true);
            await conversationStore.CreateConversation(conversation);
        }
        catch
        {
            throw;
        }
    }

    public async Task<List<ConversationResponse>> EnumerateConversations(string username)
    {
        try
        {
            await profileStore.GetProfile(username);
            var conversations = await conversationStore.EnumerateConversations(username);
            var convResponses = await profileStore.Conversation_to_ConversationResponse(username, conversations);
            return convResponses;
        }
        catch
        {
            throw;
        }
    }

    public async Task<ConvResponseWithToken> GetConversations(string username, int limit = 10, long? lastSeenConversationTime = 1, string? continuationToken = null)
    {
        try
        {
            await profileStore.GetProfile(username);
            (List<Conversation> conversations, string token) = await conversationStore.EnumerateConversations(
                username, limit, lastSeenConversationTime, WebUtility.UrlEncode(continuationToken));
            var convResponses = await profileStore.Conversation_to_ConversationResponse(username, conversations);
            return new ConvResponseWithToken(convResponses, token);
        }
        catch
        {
            throw;
        }
    }

    public async Task<long> UpdateLastModifiedTime(string conversationId, long unixTime)
    {
        List<string> usernames = conversationId.SplitToUsernames();
        try
        {
            await conversationStore.UpdateLastModifiedTime(conversationId, unixTime);
            return unixTime;
        }
        catch
        {
            throw;
        }
    }
}
