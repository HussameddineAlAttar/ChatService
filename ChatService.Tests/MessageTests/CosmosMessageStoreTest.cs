using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Tests.MessageTests;

public class CosmosMessageStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IMessagesStore messageStore;
    private readonly Message message;
    private readonly string conversationId;
    private readonly string messageId;

    private readonly string messageId_conflict;
    private readonly string conversationId_conflict;
    private readonly Message message_conflict;

    private readonly string conversationId_enum;
    private readonly string messageId_enum1;
    private readonly string messageId_enum2;
    private readonly Message message_enum1;
    private readonly Message message_enum2;

    private readonly int defaultLimit = 10;
    private readonly long defaultLastSeen = 1;
    private readonly string? nullToken = null;
    private readonly string defaultToken = "randomToken";

    public CosmosMessageStoreTest(WebApplicationFactory<Program> factory)
    {
        messageStore = factory.Services.GetRequiredService<IMessagesStore>();
        conversationId = Guid.NewGuid().ToString();
        messageId = Guid.NewGuid().ToString();
        message = new Message("FooBar", "Hello World", messageId, 123);

        conversationId_conflict = Guid.NewGuid().ToString();
        messageId_conflict = Guid.NewGuid().ToString();
        message_conflict = new Message("BarFoo", "Bye World", messageId_conflict, 456);

        conversationId_enum = Guid.NewGuid().ToString();
        messageId_enum1 = Guid.NewGuid().ToString();
        messageId_enum2 = Guid.NewGuid().ToString();
        message_enum1 = new Message("User1", "Text1", messageId_enum1, 123);
        message_enum2 = new Message("User2", "Text2", messageId_enum2, 456);
    }

    private bool EqualMessageList(List<Message> list1, List<Message> list2)
    {
        return list1.SequenceEqual(list2);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await messageStore.DeleteMessage(conversationId, messageId);
        await messageStore.DeleteMessage(conversationId_conflict, messageId_conflict);
        await messageStore.DeleteMessage(conversationId_enum, messageId_enum1);
        await messageStore.DeleteMessage(conversationId_enum, messageId_enum2);
    }

    [Fact]
    public async Task SendMessage()
    {
        await messageStore.SendMessage(conversationId, message);
        Assert.Equal(message, await messageStore.GetMessageById(conversationId, messageId));
    }

    [Fact]
    public async Task SendMessage_Conflict()
    {
        await messageStore.SendMessage(conversationId_conflict, message_conflict);   
        await Assert.ThrowsAsync<MessageConflictException>(async () =>
        {
            await messageStore.SendMessage(conversationId_conflict, message_conflict);
        });
    }

    [Fact]
    public async Task GetMessage_NotFound()
    {
        await Assert.ThrowsAsync<MessageNotFoundException>(async () =>
        {
            await messageStore.GetMessageById(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        });
    }

    [Fact]
    public async Task EnumMessages()
    {
        List<Message> myMessages = new(){message_enum2, message_enum1};

        await messageStore.SendMessage(conversationId_enum, message_enum1);
        await messageStore.SendMessage(conversationId_enum, message_enum2);

        (var actualMessages, var token) = await messageStore.EnumerateMessages(conversationId_enum, defaultLimit, defaultLastSeen, nullToken);
        Assert.True(EqualMessageList(myMessages, actualMessages));
    }

    [Fact]
    public async Task EnumMessages_NotFound()
    {
        (var actualMessages, var token) = await messageStore.EnumerateMessages(Guid.NewGuid().ToString(), defaultLimit, defaultLastSeen, nullToken);
        Assert.Empty(actualMessages);
        Assert.True(string.IsNullOrWhiteSpace(token));
    }
}
