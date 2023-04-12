using Azure.Core;
using ChatService.DTO;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ChatService.Exceptions;
using ChatService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ChatService.Tests.ConversationTests;

public class ConversationControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly CreateConvoRequest convoRequest;
    private readonly Conversation conversation;
    private readonly SendMessageRequest sendMessageRequest;
    private readonly List<string> participants;
    private readonly Message message;
    private readonly string username;

    private readonly HttpClient httpClient;
    private readonly Mock<IConversationService> conversationServiceMock = new();
    private readonly Mock<IMessageService> messageServiceMock = new();
    private ConversationController controller;

    public ConversationControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(conversationServiceMock.Object); });
            builder.ConfigureTestServices(services => { services.AddSingleton(messageServiceMock.Object); });
        }).CreateClient();
        controller = new ConversationController(conversationServiceMock.Object, messageServiceMock.Object);
        
        username = "Foo";
        participants = new List<string>{"Foo", "Bar"};
        sendMessageRequest = new(username, Guid.NewGuid().ToString());
        message = sendMessageRequest.message;
        conversation = new Conversation(participants);
        convoRequest = new CreateConvoRequest(conversation, sendMessageRequest);
    }

    [Fact]
    public async Task CreateConversation()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(convoRequest)).Returns(Task.CompletedTask);
        var response = await controller.CreateConversation(convoRequest);
        Assert.IsType<CreatedAtActionResult>(response.Result);
    }

    [Fact]
    public async Task CreateConversation_Conflict()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(convoRequest)).ThrowsAsync(new ConversationConflictException());
        var response = await controller.CreateConversation(convoRequest);
        Assert.IsType<OkObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateConversation_ProfileNotFoundException()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(convoRequest)).ThrowsAsync(new ProfileNotFoundException(new List<string>()));
        var response = await controller.CreateConversation(convoRequest);
        Assert.IsType<NotFoundObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateConversation_NotPartOfConversation()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(convoRequest)).ThrowsAsync(new NotPartOfConversationException());
        var response = await controller.CreateConversation(convoRequest);
        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task CreateConversation_TooManyParticipants()
    {
        var participants = new List<string> { "user1", "user2", "user3" };
        var conversation = new Conversation(participants);
        var request = new CreateConvoRequest(conversation, sendMessageRequest);
        var response = await controller.CreateConversation(request);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        
    }

    [Fact]
    public async Task CreateConversation_NotEnoughParticipants()
    {
        var participants = new List<string> { "user1" };
        var request = new CreateConvoRequest(new Conversation(participants), sendMessageRequest);
        var response = await controller.CreateConversation(request);

        Assert.IsType<BadRequestObjectResult>(response.Result);
    }

    [Fact]
    public async Task EnumerateConversations()
    {
        conversationServiceMock.Setup(m => m.EnumerateConversations(username))
            .ReturnsAsync(new List<ConversationResponse>());

        var response = await httpClient.GetAsync($"conversations/{username}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EnumerateConversations_ProfileNotFound()
    {
        conversationServiceMock.Setup(m => m.EnumerateConversations(username))
            .ThrowsAsync(new ProfileNotFoundException());

        var response = await httpClient.GetAsync($"conversations/{username}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EnumerateConversations_ConversationsNotFound()
    {
        conversationServiceMock.Setup(m => m.EnumerateConversations(username))
            .ThrowsAsync(new ConversationNotFoundException());

        var response = await httpClient.GetAsync($"conversations/{username}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
