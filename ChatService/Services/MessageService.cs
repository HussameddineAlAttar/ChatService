using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Extensions;
using ChatService.Storage;

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
        try
        {
            var messages = await messagesStore.EnumerateMessages(conversationId);
            return messages.Select(m => new EnumMessageResponse(m.Text, m.SenderUsername, m.Time)).ToList();
        }
        catch
        {
            throw;
        }
    }

    public async Task<(List<Message> messages, string token)> GetMessages(string conversationId, int limit = 10, long? lastSeenMessageTime = null, string? continuationToken = null)
    {
        try
        {
            return await messagesStore.EnumerateMessages(conversationId, limit, lastSeenMessageTime, continuationToken);
        }
        catch
        {
            throw;
        }
    }


    public async Task<long> SendMessage(string conversationId, Message message, bool FirstTime = false)
    {
        List<string> usernames = conversationId.SplitToUsernames();
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
            await conversationStore.UpdateLastModifiedTime(conversationId, message.Time);
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
