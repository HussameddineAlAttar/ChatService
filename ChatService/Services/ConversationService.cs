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
        await messagesStore.SendMessage(conversation.Id, convoRequest.FirstMessage.message);
        await conversationStore.CreateConversation(conversation);
    }

    public async Task<(List<EnumConvoResponse>, string token)> EnumerateConversations(string username, int limit = 10, long? lastSeenConversationTime = 1, string? continuationToken = null)
    {
        await profileStore.GetProfile(username);
        (List<Conversation> conversations, string? token) = await conversationStore.EnumerateConversations(
            username, limit, lastSeenConversationTime, continuationToken);
        var convResponses = await profileStore.GetProfilesOfParticipants(username, conversations);
        return (convResponses, token);
    }

    public async Task<long> UpdateLastModifiedTime(string conversationId, long unixTime)
    {
        List<string> usernames = conversationId.SplitToUsernames();
        await conversationStore.UpdateLastModifiedTime(conversationId, usernames, unixTime);
        return unixTime;
    }
}
