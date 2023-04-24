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

    public async Task<MessageTokenResponse> EnumerateMessages(string conversationId, int limit = 10, long lastSeenMessageTime = 1, string? continuationToken = null)
    {
        try
        {
            (var messages, var token) = await messagesStore.EnumerateMessages(conversationId, limit, lastSeenMessageTime, continuationToken);
            var messageResponses = messages.Select(message =>
            new EnumMessageResponse(message.Text, message.SenderUsername, message.Time))
                .ToList();
            var messageTokenResponse = new MessageTokenResponse(messageResponses, conversationId, limit, lastSeenMessageTime, token);
            return messageTokenResponse;
        }
        catch { throw; }
    }

    public async Task<long> SendMessage(string conversationId, Message message)
    {
        List<string> usernames = conversationId.SplitToUsernames();
        if (!usernames.Contains(message.SenderUsername))
        {
            throw new NotPartOfConversationException($"Sender {message.SenderUsername} is not part of the conversation {conversationId}");
        }
        try
        {
            await conversationStore.UpdateLastModifiedTime(conversationId, usernames, message.Time);
            await messagesStore.SendMessage(conversationId, message);
            return message.Time;
        }
        catch { throw; }

    }
}
