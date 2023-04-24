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
    private readonly List<string> usernames;

    private readonly Message message1;
    private readonly Message message2;
    private readonly List<Message> messages;

    private readonly int defaultLimit = 10;
    private readonly long defaultLastSeen = 1;
    private readonly string? nullToken = null;
    private readonly string defaultToken = "randomToken";

    public MessageServiceTest()
    {
        messageService = new MessageService(messageStoreMock.Object, conversationStoreMock.Object);
        conversationId = "FooBar_NewFooBar";
        usernames = conversationId.SplitToUsernames();
        
        message1 = new Message("FooBar", "Hello World", Guid.NewGuid().ToString(), 123);
        message2 = new Message("NewFooBar", "Goodbye", Guid.NewGuid().ToString(), 456);
        messages = new List<Message> { message1, message2 };
    }

    [Fact]
    public async Task EnumerateMessages()
    {
        messageStoreMock.Setup(x => x.EnumerateMessages(conversationId, defaultLimit, defaultLastSeen, nullToken))
            .ReturnsAsync((messages, defaultToken));
        var result = await messageService.EnumerateMessages(conversationId);
        string expectedUri = $"/api/conversations/{conversationId}/messages?limit={defaultLimit}&lastSeenMessageTime={defaultLastSeen}&continuationToken={defaultToken}";

        Assert.Equal(expectedUri, result.NextUri);
        // assert list of messages are equal
    }

    [Fact]
    public async Task EnumerateMessages_ConversationNotFound()
    {
        messageStoreMock.Setup(x => x.EnumerateMessages(conversationId, defaultLimit, defaultLastSeen, nullToken))
            .ReturnsAsync((new List<Message>() { }, nullToken));
        var result = await messageService.EnumerateMessages(conversationId);

        Assert.True(string.IsNullOrWhiteSpace(result.NextUri));
        Assert.Empty(result.Messages);
    }

    [Fact]
    public async Task SendMessage()
    {
        messageStoreMock.Setup(x => x.SendMessage(conversationId, message1)).Returns(Task.CompletedTask);
        conversationStoreMock.Setup(x => x.UpdateLastModifiedTime(conversationId, usernames, message1.Time)).Returns(Task.CompletedTask);

        var actualSentTime = await messageService.SendMessage(conversationId, message1);
        Assert.Equal(message1.Time, actualSentTime);
    }

    [Fact]
    public async Task SendMessage_SenderNotPartOfConversation()
    {
        await Assert.ThrowsAsync<NotPartOfConversationException>(async () =>
        {
            await messageService.SendMessage("User1_User2", message1);
        });
    }

    [Fact]
    public async Task SendMessage_ConversationNotFound()
    {
        conversationStoreMock.Setup(x => x.UpdateLastModifiedTime(conversationId, usernames, It.IsAny<long>())).ThrowsAsync(new ConversationNotFoundException());
        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
        {
            await messageService.SendMessage(conversationId, message1);
        });
        messageStoreMock.Verify(x => x.SendMessage(conversationId, message1), Times.Never);
    }
}