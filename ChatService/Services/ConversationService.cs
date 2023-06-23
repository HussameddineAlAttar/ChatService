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

    public async Task CreateConversation(CreateConversationRequest convoRequest)
    {
        var conversation = new Conversation(convoRequest.Participants);
        var NonExistingProfiles = await profileStore.CheckForNonExistingProfile(conversation.Participants);
        if(NonExistingProfiles.Count > 0)
        {
            throw new ProfileNotFoundException(NonExistingProfiles);
        }

        bool senderValid = conversation.CheckSenderValidity(convoRequest.FirstMessage.SenderUsername);
        if (!senderValid){
            throw new NotPartOfConversationException($"Sender {convoRequest.FirstMessage.SenderUsername} is not part of the conversation {conversation.Id}");
        }

        await messagesStore.SendMessage(conversation.Id, convoRequest.FirstMessage.message);
        await conversationStore.CreateConversation(conversation);
    }


    public async Task<(List<EnumerateConversationsEntry>, string token)> EnumerateConversations(string username, int limit = 10, long? lastSeenConversationTime = 1, string? continuationToken = null)
    {
        var profileTask = profileStore.GetProfile(username);
        var conversationsTask = conversationStore.EnumerateConversations(username, limit, lastSeenConversationTime, continuationToken);

        await Task.WhenAll(profileTask, conversationsTask);
        /* 
            conversationsTask would return an empty list if there are no conversations or if the profile doesn't exist,
            profileTask would throw an error if the profile doesn't exist. We can run them in parallel and be sure that
            a non-existing profile would throw an error, while a profile with no new conversations would return 
            an empty list. This allows continuous GET requests to happen quicker without impacting functionality.
         */
        var (conversations, token) = await conversationsTask;

        var convResponses = await profileStore.GetProfilesOfParticipants(username, conversations);

        return (convResponses, token);
    }
}