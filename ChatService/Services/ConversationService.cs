using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Extensions;
using ChatService.Storage.Interfaces;

namespace ChatService.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationStore conversationStore;
    private readonly IMessageService messageService;
    private readonly IProfileInterface profileStore;

    public ConversationService(IConversationStore _conversationStore, IProfileInterface _profileStore, IMessageService _messageService)
    {
        conversationStore = _conversationStore;
        profileStore = _profileStore;
        messageService = _messageService;
    }

    public async Task CreateConversation(CreateConvoRequest convoRequest)
    {
        var conversation = convoRequest.Conversation;
        var NonExistingProfiles = await profileStore.CheckFor_NonExistingProfile(conversation.Participants);
        if(NonExistingProfiles.Count > 0)
        {
            throw new ProfileNotFoundException(NonExistingProfiles);
        }
        try
        {
            await messageService.SendMessage(conversation.Id, convoRequest.FirstMessageRequest.message, true);
            for(int i = 0; i < conversation.Participants.Count; i++)
            {
                await conversationStore.CreateConversation(conversation, conversation.Participants[i]);
            }
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

    public async Task<long> ModifyTime(string conversationId, long time)
    {
        List<string> usernames = conversationId.Split("_").ToList();
        try
        {
            for (int i = 0; i < usernames.Count; i++)
            {
                await conversationStore.ModifyTime(usernames[i], conversationId, time);
            }
            return time;
        }
        catch
        {
            throw;
        }
    }
}
