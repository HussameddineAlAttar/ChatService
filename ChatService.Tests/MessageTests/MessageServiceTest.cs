using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Extensions;
using ChatService.Services;
using ChatService.Storage.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace ChatService.Tests.MessageTests;

public class MessageServiceTest
{
    private readonly Mock<IMessagesStore> messageStoreMock = new();
    private readonly Mock<IConversationStore> conversationStoreMock = new();
    private readonly MessageService messageService;

    private readonly string conversationId;
    private readonly Message message1;
    private readonly Message message2;
    private readonly List<Message> messages;

    public MessageServiceTest()
    {
        messageService = new MessageService(messageStoreMock.Object, conversationStoreMock.Object);
        conversationId = "FooBar_NewFooBar";
        message1 = new Message("FooBar", "Hello World", Guid.NewGuid().ToString(), 123);
        message2 = new Message("NewFooBar", "Goodbye", Guid.NewGuid().ToString(), 456);
        messages = new List<Message> { message1, message2 };
    }

    [Fact]
    public async Task EnumerateMessages()
    {
        messageStoreMock.Setup(x => x.EnumerateMessages(conversationId)).ReturnsAsync(messages);
        var result = await messageService.EnumerateMessages(conversationId);
        Assert.Equal(messages.Count, result.Count);
        for (int i = 0; i < messages.Count; i++)
        {
            EnumMessageResponse response = new(messages[i].Text, messages[i].SenderUsername, messages[i].Time);
            Assert.Equal(response, result[i]);
        }
    }

    [Fact]
    public async Task SendMessage()
    {
        messageStoreMock.Setup(x => x.SendMessage(conversationId, message1)).Returns(Task.CompletedTask);
        conversationStoreMock.Setup(x => x.ModifyTime("user1", conversationId, message1.Time)).Returns(Task.CompletedTask);
        conversationStoreMock.Setup(x => x.ModifyTime("user2", conversationId, message1.Time)).Returns(Task.CompletedTask);

        var result = await messageService.SendMessage(conversationId, message1);
        Assert.Equal(message1.Time, result);
    }

    [Fact]
    public async Task SendMessage_FirstTime()
    {
        messageStoreMock.Setup(x => x.SendMessage(conversationId, message1)).ThrowsAsync(new ConversationNotFoundException());
        var result = await messageService.SendMessage(conversationId, message1, true);
        Assert.Equal(message1.Time, result);
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
    public async Task SendMessage_NotFirstTime()
    {
        conversationStoreMock.Setup(m => m.FindConversationById(conversationId)).ThrowsAsync(new ConversationNotFoundException());
        await Assert.ThrowsAsync<ConversationNotFoundException>(() => messageService.SendMessage(conversationId, message1));
    }

}
