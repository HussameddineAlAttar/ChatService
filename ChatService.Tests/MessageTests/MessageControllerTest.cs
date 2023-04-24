using ChatService.Controllers;
using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace ChatService.Tests.MessageTests;

public class MessageControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly Mock<IMessageService> messageServiceMock = new();
    private readonly HttpClient httpClient;

    private readonly SendMessageRequest messageRequest;
    private readonly Message message;
    private readonly EnumMessageResponse enumMessage1;
    private readonly EnumMessageResponse enumMessage2;

    private readonly List<EnumMessageResponse> messagesList;
    private readonly MessageTokenResponse messageTokenResponse;
    private readonly string conversationId;

    private readonly int defaultLimit = 10;
    private readonly long defaultLastSeen = 1;
    private readonly string? nullToken = null;
    private readonly string defaultToken = "randomToken";

    public MessageControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(messageServiceMock.Object); });
        }).CreateClient();
        messageRequest = new SendMessageRequest(Guid.NewGuid().ToString(), "FooBar", "Hello World");
        conversationId = Guid.NewGuid().ToString();

        enumMessage1 = new("Hello", "FooBar", 123);
        enumMessage2 = new("Bye", "FizzBuzz", 456);
        messageTokenResponse = new(new List<EnumMessageResponse>() { enumMessage2, enumMessage1 },
            conversationId, defaultLimit, defaultLastSeen, defaultToken);
        message = messageRequest.message;
        conversationId = Guid.NewGuid().ToString();
    }

    private bool EqualMessage(Message msg1, Message msg2)
    {
        return msg1.Text == msg2.Text && msg1.SenderUsername == msg2.SenderUsername && msg1.Id == msg2.Id;
    }
    private bool EqualMessageList(List<EnumMessageResponse> list1, List<EnumMessageResponse> list2)
    {
        return list1.SequenceEqual(list2);
    }

    [Fact]
    public async Task SendMessage()
    {
        messageServiceMock.Setup(m => m.SendMessage(conversationId, It.Is<Message>(msg => EqualMessage(msg, message))))
            .ReturnsAsync(message.Time);
        var httpResponse = await httpClient.PostAsync($"/api/conversations/{conversationId}/messages",
            new StringContent(JsonConvert.SerializeObject(messageRequest), Encoding.Default, "application/json"));
        var json = await httpResponse.Content.ReadAsStringAsync();
        var sendMessageResponse = JsonConvert.DeserializeObject<SendMessageResponse>(json);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Equal(message.Time, sendMessageResponse.CreatedUnixTime);
    }

    [Fact]
    public async Task SendMessage_Conflict()
    {
        messageServiceMock.Setup(m => m.SendMessage(conversationId, It.Is<Message>(msg => EqualMessage(msg, message))))
            .ThrowsAsync(new MessageConflictException());
        var httpResponse = await httpClient.PostAsync($"/api/conversations/{conversationId}/messages",
            new StringContent(JsonConvert.SerializeObject(messageRequest), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Conflict, httpResponse.StatusCode);
    }

    [Fact]
    public async Task SendMessage_ConversationNotFound()
    {
        messageServiceMock.Setup(m => m.SendMessage(conversationId, It.Is<Message>(msg => EqualMessage(msg, message))))
            .ThrowsAsync(new ConversationNotFoundException());
        var httpResponse = await httpClient.PostAsync($"/api/conversations/{conversationId}/messages",
                    new StringContent(JsonConvert.SerializeObject(messageRequest), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task SendMessage_SenderNotPartOfConversation()
    {
        messageServiceMock.Setup(m => m.SendMessage(conversationId, It.Is<Message>(msg => EqualMessage(msg, message))))
            .ThrowsAsync(new NotPartOfConversationException());
        var httpResponse = await httpClient.PostAsync($"/api/conversations/{conversationId}/messages",
                    new StringContent(JsonConvert.SerializeObject(messageRequest), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task EnumerateMessages()
    {
        messageServiceMock.Setup(m => m.EnumerateMessages(conversationId, defaultLimit, defaultLastSeen, nullToken))
            .ReturnsAsync(messageTokenResponse);

        var response = await httpClient.GetAsync($"/api/conversations/{conversationId}/messages");
        var json = await response.Content.ReadAsStringAsync();
        var receivedMessageResponse = JsonConvert.DeserializeObject<MessageTokenResponse>(json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(messageTokenResponse.NextUri, receivedMessageResponse.NextUri);
        Assert.True(EqualMessageList(messageTokenResponse.Messages, receivedMessageResponse.Messages));

    }

    [Fact]
    public async Task EnumerateMessages_ConversationNotFound()
    {
        messageServiceMock.Setup(m => m.EnumerateMessages(conversationId, defaultLimit, defaultLastSeen, nullToken))
             .ThrowsAsync(new ConversationNotFoundException());

        var response = await httpClient.GetAsync($"/api/conversations/{conversationId}/messages");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
