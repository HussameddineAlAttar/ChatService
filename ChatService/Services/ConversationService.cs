using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Extensions;
using ChatService.Storage;

namespace ChatService.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationStore conversationStore;
    private readonly IMessagesStore messagesStore;
    private readonly IProfileStore profileStore;

    public ConversationService(IConversationStore _conversationStore, IProfileStore _profileStore, IMessagesStore _messageStore)
    {
        conversationStore = _conversationStore;
        profileStore = _profileStore;
        messagesStore = _messageStore;
    }

    public async Task CreateConversation(CreateConvoRequest convoRequest)
    {
        var conversation = new Conversation(convoRequest.Participants);
        var NonExistingProfiles = await profileStore.CheckForNonExistingProfile(conversation.Participants);
        if(NonExistingProfiles.Count > 0)
        {
            throw new ProfileNotFoundException(NonExistingProfiles);
        }
        try
        {
            await messagesStore.SendMessage(conversation.Id, convoRequest.FirstMessage.message);
            await conversationStore.CreateConversation(conversation);
        }
        catch { throw; }
    }

    public async Task<ConvoResponseWithToken> EnumerateConversations(string username, int limit = 10, long? lastSeenConversationTime = 1, string? continuationToken = null)
    {
        try
        {
            await profileStore.GetProfile(username);
            (List<Conversation> conversations, string? token) = await conversationStore.EnumerateConversations(
                username, limit, lastSeenConversationTime, continuationToken);
            var convResponses = await profileStore.Conversation_to_ConversationResponse(username, conversations);
            ConvoResponseWithToken responseWithUri = new(convResponses, username, limit, lastSeenConversationTime, token);
            return responseWithUri;
        }
        catch { throw; }
    }

    public async Task<long> UpdateLastModifiedTime(string conversationId, long unixTime)
    {
        List<string> usernames = conversationId.SplitToUsernames();
        try
        {
            await conversationStore.UpdateLastModifiedTime(conversationId, usernames, unixTime);
            return unixTime;
        }
        catch { throw; }
    }
}
