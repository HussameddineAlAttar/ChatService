using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Extensions;
using ChatService.Services;
using ChatService.Storage;
using Microsoft.VisualBasic;
using Moq;

namespace ChatService.Tests.MessageTests;

public class MessageServiceTest
{
    private readonly Mock<IMessagesStore> messageStoreMock = new();
    private readonly Mock<IConversationStore> conversationStoreMock = new();
    private readonly MessageService messageService;

    private readonly string conversationId;
    private readonly string conversationIdForNotPart;

    private readonly List<string> usernames;
    private readonly List<string> usernamesForNotPart;

    private readonly Conversation conversation;
    private readonly Conversation conversationForNotPart;

    private readonly Message message1;
    private readonly Message message2;
    private readonly List<Message> messages;
    private readonly List<EnumerateMessagesEntry> enumerateMessages;

    private readonly int defaultLimit = 10;
    private readonly long defaultLastSeen = 1;
    private readonly string? nullToken = null;
    private readonly string defaultToken = "randomToken";

    public MessageServiceTest()
    {
        messageService = new MessageService(messageStoreMock.Object, conversationStoreMock.Object);
        usernames = new() { "FooBar", "NewFooBar" };
        conversation = new(usernames);
        conversationId = conversation.Id;

        usernamesForNotPart = new() { "User1", "User2" };
        conversationForNotPart = new(usernamesForNotPart);
        conversationIdForNotPart = conversationForNotPart.Id;
        
        message1 = new Message("FooBar", "Hello World", Guid.NewGuid().ToString(), 123);
        message2 = new Message("NewFooBar", "Goodbye", Guid.NewGuid().ToString(), 456);
        messages = new() { message2, message1 };
        enumerateMessages = new()
        {
            new EnumerateMessagesEntry(message2.Text, message2.SenderUsername, message2.Time),
            new EnumerateMessagesEntry(message1.Text, message1.SenderUsername, message1.Time)
        };
    }

    private bool EqualMessagesList(List<EnumerateMessagesEntry> list1, List<EnumerateMessagesEntry> list2)
    {
        for (int i = 0; i < list1.Count; ++i)
        {
            if (list1[i].Text != list2[i].Text || list1[i].SenderUsername != list2[i].SenderUsername)
            {
                return false;
            }
        }
        return true;
    }

    [Fact]
    public async Task EnumerateMessages()
    {
        messageStoreMock.Setup(x => x.EnumerateMessages(conversationId, defaultLimit, defaultLastSeen, nullToken))
            .ReturnsAsync((messages, defaultToken));
        (var messageListResponse, var token) = await messageService.EnumerateMessages(conversationId);

        Assert.Equal(defaultToken, token);
        Assert.True(EqualMessagesList(enumerateMessages, messageListResponse));
    }

    [Fact]
    public async Task EnumerateMessages_NoMoreMessages()
    {
        messageStoreMock.Setup(x => x.EnumerateMessages(conversationId, defaultLimit, defaultLastSeen, nullToken))
            .ReturnsAsync((new List<Message>() { }, nullToken));
        (var messageListResponse, var token) = await messageService.EnumerateMessages(conversationId);

        Assert.True(string.IsNullOrWhiteSpace(token));
        Assert.Empty(messageListResponse);
    }

    [Fact]
    public async Task EnumerateMessages_ConversationNotFound()
    {
        conversationStoreMock.Setup(x => x.FindConversationById(conversationId)).ThrowsAsync(new ConversationNotFoundException());
        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
        {
            await messageService.EnumerateMessages(conversationId);
        });
        messageStoreMock.Verify(x => x.EnumerateMessages(conversationId, defaultLimit, defaultLastSeen, defaultToken), Times.Never);
    }

    [Fact]
    public async Task SendMessage()
    {
        conversationStoreMock.Setup(x => x.FindConversationById(conversationId)).ReturnsAsync(conversation);
        messageStoreMock.Setup(x => x.SendMessage(conversationId, message1)).Returns(Task.CompletedTask);
        conversationStoreMock.Setup(x => x.UpdateLastModifiedTime(conversationId, usernames, message1.Time)).Returns(Task.CompletedTask);

        var actualSentTime = await messageService.SendMessage(conversationId, message1);
        Assert.Equal(message1.Time, actualSentTime);
    }

    [Fact]
    public async Task SendMessage_SenderNotPartOfConversation()
    {
        conversationStoreMock.Setup(x => x.FindConversationById(conversationIdForNotPart)).ReturnsAsync(conversationForNotPart);
        await Assert.ThrowsAsync<NotPartOfConversationException>(async () =>
        {
            await messageService.SendMessage(conversationIdForNotPart, message1);
        });
    }

    [Fact]
    public async Task SendMessage_ConversationNotFound()
    {
        conversationStoreMock.Setup(x => x.FindConversationById(conversationId)).ThrowsAsync(new ConversationNotFoundException());
        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
        {
            await messageService.SendMessage(conversationId, message1);
        });
        messageStoreMock.Verify(x => x.SendMessage(conversationId, message1), Times.Never);
        conversationStoreMock.Verify(x => x.UpdateLastModifiedTime(conversationId, usernames, It.IsAny<long>()), Times.Never);
    }
}