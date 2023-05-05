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
        var conversationTask = conversationStore.FindConversationById(conversationId);
        var messagesTask = messagesStore.EnumerateMessages(conversationId, limit, lastSeenMessageTime, continuationToken);

        await Task.WhenAll(conversationTask, messagesTask);
        /* 
            messagesTask would return an empty list if there are no new messages or if the conversation doesn't exist,
            conversationTask would throw an error if the conversation doesn't exist. We can run them in
            parallel and be sure that a non-existing conversation would throw an error, while a conversation
            with no new messages would return an empty list. This allows continuous GET requests to happen
            quicker without impacting functionality.
         */
        var (messages, token) = await messagesTask;

        var messageResponses = messages.Select(message =>
            new EnumerateMessagesEntry(message.Text, message.SenderUsername, message.Time))
            .ToList();

        return (messageResponses, token);
    }


    public async Task<long> SendMessage(string conversationId, Message message)
    {
        var conversation = await conversationStore.FindConversationById(conversationId);
        List<string> usernames = conversation.Participants;
        if (!usernames.Contains(message.SenderUsername))
        {
            throw new NotPartOfConversationException($"Sender {message.SenderUsername} is not part of the conversation {conversationId}");
        }
        await Task.WhenAll(
            conversationStore.UpdateLastModifiedTime(conversationId, usernames, message.Time),
            messagesStore.SendMessage(conversationId, message)
        );
        return message.Time;
    }
}