using ChatService.Controllers;
using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using ChatService.Storage.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Tests.MessageTests;

public class MessageControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IMessageService> messageServiceMock = new();
    private readonly HttpClient httpClient;
    private readonly MessageController controller;

    private readonly SendMessageRequest request;
    private readonly Message message;
    private readonly List<EnumMessageResponse> messagesList;
    private readonly string conversationId;

    public MessageControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(messageServiceMock.Object); });
        }).CreateClient();
        controller = new MessageController(messageServiceMock.Object);
        request = new SendMessageRequest("FooBar", "Hello World");
        message = request.message;
        conversationId = Guid.NewGuid().ToString();
        messagesList = new List<EnumMessageResponse>();
    }

    [Fact]
    public async Task SendMessage()
    {
        messageServiceMock.Setup(m => m.SendMessage(conversationId, message, false)).ReturnsAsync(message.Time);
        var response = await controller.SendMessage(conversationId, request);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(response.Result);
        var returnedTime = Assert.IsType<long>(createdAtActionResult.Value);
        Assert.Equal(message.Time, returnedTime);
        messageServiceMock.Verify(m => m.SendMessage(conversationId, message, false), Times.Once);
    }

    [Fact]
    public async Task SendMessage_Conflict()
    { 

        messageServiceMock.Setup(m => m.SendMessage(conversationId, message, false)).ThrowsAsync(new MessageConflictException());
        var response = await controller.SendMessage(conversationId, request);
        var conflictResult = Assert.IsType<ConflictObjectResult>(response.Result);
        messageServiceMock.Verify(m => m.SendMessage(conversationId, message, false), Times.Once);

    }

    [Fact]
    public async Task SendMessage_ConversationNotFound()
    {
        messageServiceMock.Setup(m => m.SendMessage(conversationId, message, false)).ThrowsAsync(new ConversationNotFoundException());
        var response = await controller.SendMessage(conversationId, request);
        var conflictResult = Assert.IsType<NotFoundObjectResult>(response.Result);
        messageServiceMock.Verify(m => m.SendMessage(conversationId, message, false), Times.Once);
    }

    [Fact]
    public async Task SendMessage_SenderNotPartOfConversation()
    {
        messageServiceMock.Setup(m => m.SendMessage(conversationId, message, false)).ThrowsAsync(new NotPartOfConversationException());
        var response = await controller.SendMessage(conversationId, request);
        var badRequest = Assert.IsType<BadRequestObjectResult>(response.Result);
        messageServiceMock.Verify(m => m.SendMessage(conversationId, message, false), Times.Once);
    }

    [Fact]
    public async Task EnumerateMessages()
    {
        messageServiceMock.Setup(m => m.EnumerateMessages(conversationId))
            .ReturnsAsync(messagesList);

        var response = await httpClient.GetAsync($"/conversations/{conversationId}/messages");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Equal(messagesList, JsonConvert.DeserializeObject<List<EnumMessageResponse>>(json));
        messageServiceMock.Verify(mock => mock.EnumerateMessages(conversationId), Times.Once());
    }

    [Fact]
    public async Task EnumerateMessages_ConversationNotFound()
    {
        messageServiceMock.Setup(m => m.EnumerateMessages(conversationId))
             .ThrowsAsync(new ConversationNotFoundException());

        var response = await httpClient.GetAsync($"/conversations/{conversationId}/messages");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        messageServiceMock.Verify(mock => mock.EnumerateMessages(conversationId), Times.Once());
    }

}
