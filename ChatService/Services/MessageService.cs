using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Extensions;
using ChatService.Storage.Interfaces;

namespace ChatService.Services;

public class MessageService : IMessageService
{
    private readonly IMessagesStore messagesStore;
    private readonly IConversationStore conversationStore;

    public MessageService(IMessagesStore _messagesStore, IConversationStore _conversationStore)
    {
        messagesStore = _messagesStore;
        conversationStore = _conversationStore;
    }

    public async Task<List<EnumMessageResponse>> EnumerateMessages(string conversationId)
    {
        List<EnumMessageResponse> messageResponses = new();
        try
        {
            var messages = await messagesStore.EnumerateMessages(conversationId);
            for(int i = 0; i < messages.Count; i++)
            {
                EnumMessageResponse response = new(messages[i].Text, messages[i].SenderUsername, messages[i].Time);
                messageResponses.Add(response);
            }
            return messageResponses;
        }
        catch
        {
            throw;
        }
    }

    public async Task<MessageTokenResponse> GetMessages(string conversationId, int limit = 10, long? lastSeenMessageTime = null, string? continuationToken = null)
    {
        List<EnumMessageResponse> messageResponses = new();
        try
        {
            (var messages, string token) = await messagesStore.EnumerateMessages(conversationId, limit, lastSeenMessageTime, continuationToken);
            for (int i = 0; i < messages.Count; i++)
            {
                EnumMessageResponse response = new(messages[i].Text, messages[i].SenderUsername, messages[i].Time);
                messageResponses.Add(response);
            }
            return new MessageTokenResponse(messageResponses, token);
        }
        catch
        {
            throw;
        }
    }

    public async Task<long> SendMessage(string conversationId, Message message, bool FirstTime = false)
    {
        List<string> usernames = conversationId.Split("_").ToList();
        if (!usernames.Contains(message.SenderUsername))
        {
            throw new NotPartOfConversationException();
        }
        bool conversationExists = await conversationStore.CheckIfConversationExists(conversationId);
        if (!conversationExists && !FirstTime)
        {
            throw new ConversationNotFoundException();
        }
        try
        {
            await messagesStore.SendMessage(conversationId, message);
            
            for (int i = 0; i < usernames.Count; i++)
            {
                await conversationStore.ModifyTime(usernames[i], conversationId, message.Time);
            }
            return message.Time;
        }
        catch (Exception e)
        {
            if (e is ConversationNotFoundException)
            {
                return message.Time;
            }
            throw;
        }

    }
}
