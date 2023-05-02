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

    public async Task<(List<EnumerateMessagesEntry>, string token)> EnumerateMessages(string conversationId, int limit = 10, long lastSeenMessageTime = 1, string? continuationToken = null)
    {
        List<string> usernames = conversationId.SplitToUsernames();
        await conversationStore.FindConversationById(conversationId, usernames[0]); // can use the username of any participant to check if convo exists
        (var messages, var token) = await messagesStore.EnumerateMessages(conversationId, limit, lastSeenMessageTime, continuationToken);
        var messageResponses = messages.Select(message =>
        new EnumerateMessagesEntry(message.Text, message.SenderUsername, message.Time))
            .ToList();
        return (messageResponses, token);
    }

    public async Task<long> SendMessage(string conversationId, Message message)
    {
        List<string> usernames = conversationId.SplitToUsernames();
        if (!usernames.Contains(message.SenderUsername))
        {
            throw new NotPartOfConversationException($"Sender {message.SenderUsername} is not part of the conversation {conversationId}");
        }
        await conversationStore.FindConversationById(conversationId, usernames[0]);
        await Task.WhenAll(
            conversationStore.UpdateLastModifiedTime(conversationId, usernames, message.Time),
            messagesStore.SendMessage(conversationId, message)
        );
        return message.Time;
    }
}
