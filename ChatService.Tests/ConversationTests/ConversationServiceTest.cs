//using ChatService.DTO;
//using ChatService.Exceptions;
//using ChatService.Extensions;
//using ChatService.Services;
//using ChatService.Storage;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Extensions.DependencyInjection;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq.Expressions;
//using System.Threading.Tasks;
//using Xunit;

//namespace ChatService.Tests.ConversationTests;

//public class ConversationServiceTest
//{
//    private readonly Mock<IMessageService> messageServiceMock = new();
//    private readonly Mock<IConversationStore> conversationStoreMock = new();
//    private readonly Mock<IProfileStore> profileStoreMock = new();
//    private readonly ConversationService conversationService;

//    private readonly CreateConvoRequest convoRequest;
//    private readonly Conversation conversation;
//    private readonly SendMessageRequest sendMessageRequest;
//    private readonly List<string> participants;
//    private readonly Message message;
//    private readonly string username;
//    private readonly Profile testProfile;

//    public ConversationServiceTest()
//    {
//        conversationService = new ConversationService(conversationStoreMock.Object, profileStoreMock.Object, messageServiceMock.Object);
//        username = "Foo";
//        participants = new List<string> { "Foo", "Bar" };
//        sendMessageRequest = new(username, Guid.NewGuid().ToString());
//        message = sendMessageRequest.message;
//        conversation = new Conversation(participants);
//        convoRequest = new CreateConvoRequest(participants, sendMessageRequest);
//        testProfile = new Profile(username, "FirstName", "LastName", Guid.NewGuid().ToString());
//    }

//    [Fact]
//    public async Task CreateConversation()
//    {
//        messageServiceMock.Setup(x => x.SendMessage(conversation.Id, message, true)).ReturnsAsync(123);
//        await conversationService.CreateConversation(convoRequest);

//        messageServiceMock.Verify(x => x.SendMessage(conversation.Id, message, true), Times.Once);
//        conversationStoreMock.Verify(x => x.CreateConversation(new Conversation(convoRequest.Participants)), Times.Once);
//    }

//    [Fact]
//    public async Task CreateConversation_ProfileNotFound()
//    {
//        profileStoreMock.Setup(x => x.GetProfile(username)).ThrowsAsync(new ProfileNotFoundException());
//        await Assert.ThrowsAsync<ProfileNotFoundException>(async () =>
//        {
//            await conversationService.CreateConversation(convoRequest);
//        });
//        messageServiceMock.Verify(x => x.SendMessage(conversation.Id, message, true), Times.Never);
//    }

//    [Fact]
//    public async Task EnumerateConversations()
//    {
//        var conversationList = new List<Conversation>{conversation};
//        Profile Recipient = new("Bar", "BarFirst", "BarLast", Guid.NewGuid().ToString());
//        var conversationResponseList = new List<ConversationResponse>
//        {
//            new ConversationResponse(conversation.Id, conversation.ModifiedTime, Recipient)
//        };

//        profileStoreMock.Setup(x => x.GetProfile(username)).ReturnsAsync(testProfile);
//        conversationStoreMock.Setup(x => x.EnumerateConversations(username)).ReturnsAsync(conversationList);
//        var response = await conversationService.EnumerateConversations(username);

//        Assert.Equal(conversationResponseList.Count, response.Count);
//        for(int i = 0; i < conversationResponseList.Count; i++)
//        {
//            Assert.Equal(conversationResponseList[i].Id, response[i].Id);
//            Assert.Equal(conversationResponseList[i].LastModifiedUnixTime, response[i].LastModifiedUnixTime);
//            Assert.Equal(conversationResponseList[i].Recipient, response[i].Recipient);
//        }
//        conversationStoreMock.Verify(x => x.EnumerateConversations(username), Times.Once);
//    }

//    [Fact]
//    public async Task EnumerateConversations_UserNotFound()
//    {
//        profileStoreMock.Setup(x => x.GetProfile(username)).ThrowsAsync(new ProfileNotFoundException());
//        await Assert.ThrowsAsync<ProfileNotFoundException>(async () =>
//        {
//            await conversationService.EnumerateConversations(username);
//        });
//        conversationStoreMock.Verify(x => x.EnumerateConversations(username), Times.Never);
//    }

//    [Fact]
//    public async Task EnumerateConversations_ConversationsNotFound()
//    {
//        profileStoreMock.Setup(x => x.GetProfile(username)).ReturnsAsync(testProfile);
//        conversationStoreMock.Setup(x => x.EnumerateConversations(username)).ThrowsAsync(new ConversationNotFoundException());
//        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
//        {
//            await conversationService.EnumerateConversations(username);
//        });
//        conversationStoreMock.Verify(x => x.EnumerateConversations(username), Times.Once);
//    }

//    [Fact]
//    public async Task ModifyTime()
//    {
//        conversationStoreMock.Setup(x => x.UpdateLastModifiedTime(conversation.Id, 123)).Returns(Task.CompletedTask);
//        var time = await conversationService.UpdateLastModifiedTime(conversation.Id, 123);
//        Assert.Equal(123, time);
//    }

//    [Fact]
//    public async Task ModifyTime_ConversationNotFound()
//    {
//        conversationStoreMock.Setup(x => x.UpdateLastModifiedTime(conversation.Id, It.IsAny<long>())).ThrowsAsync(new ConversationNotFoundException());
//        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
//        {
//            await conversationService.UpdateLastModifiedTime(conversation.Id, 123);
//        });
//    }
//}
