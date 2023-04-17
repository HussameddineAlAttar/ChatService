using ChatService.DTO;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using ChatService.Exceptions;
using ChatService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;

namespace ChatService.Tests.ConversationTests;

public class ConversationControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly CreateConvoRequest convoRequest;
    private readonly Conversation conversation;
    private readonly SendMessageRequest sendMessageRequest;
    private readonly EnumMessageResponse enumMessageResponse1;
    private readonly EnumMessageResponse enumMessageResponse2;
    private readonly MessageTokenResponse messageTokenResponse;
    private readonly List<string> participants;
    private readonly Message message;
    private readonly string username;

    private readonly HttpClient httpClient;
    private readonly Mock<IConversationService> conversationServiceMock = new();
    private readonly Mock<IMessageService> messageServiceMock = new();
    private ConversationController controller;

    private readonly int defaultLimit = 1;
    private readonly long defaultLastSeen = 1;
    private readonly string defaultToken = "randomToken";

    public ConversationControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(conversationServiceMock.Object); });
            builder.ConfigureTestServices(services => { services.AddSingleton(messageServiceMock.Object); });
        }).CreateClient();
        controller = new ConversationController(conversationServiceMock.Object, messageServiceMock.Object);

        username = "Foo";
        participants = new List<string> { "Foo", "Bar" };
        sendMessageRequest = new(Guid.NewGuid().ToString(),username, Guid.NewGuid().ToString());
        message = sendMessageRequest.message;
        conversation = new Conversation(participants);
        convoRequest = new CreateConvoRequest(participants, sendMessageRequest);
        enumMessageResponse1 = new("Hello World", "Foo", 1000);
        enumMessageResponse2 = new("Bye World", "Bar", 2000);
        messageTokenResponse = new(new List<EnumMessageResponse>() {enumMessageResponse2, enumMessageResponse1 },
            conversation.Id, defaultLimit, defaultLastSeen, defaultToken);
    }

    public bool EqualMessageList(List<EnumMessageResponse> list1, List<EnumMessageResponse> list2)
    {
        return list1.SequenceEqual(list2);
    }

    [Fact]
    public async Task CreateConversation()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(convoRequest)).Returns(Task.CompletedTask);
        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(convoRequest), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
    }

    [Fact]
    public async Task CreateConversation_Conflict()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(It.IsAny<CreateConvoRequest>())).ThrowsAsync(new ConversationConflictException());
        messageServiceMock.Setup(m => m.EnumerateMessages(conversation.Id, It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(messageTokenResponse);

        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(convoRequest), Encoding.Default, "application/json"));
        var json = await httpResponse.Content.ReadAsStringAsync();
        var messagesWithUri = JsonConvert.DeserializeObject<MessageTokenResponse>(json);

        string expectedUri = $"/api/conversations/{conversation.Id}/messages?limit={defaultLimit}&lastSeenMessageTime={defaultLastSeen}&continuationToken={defaultToken}";
        
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.Equal(expectedUri, messagesWithUri.NextUri);
        Assert.True(EqualMessageList(messageTokenResponse.Messages, messagesWithUri.Messages));
    }

    [Fact]
    public async Task CreateConversation_ProfileNotFoundException()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(It.IsAny<CreateConvoRequest>())).ThrowsAsync(new ProfileNotFoundException(new List<string>() {"BarFoo", "BooFar"}));
        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(convoRequest), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task CreateConversation_NotPartOfConversation()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(It.IsAny<CreateConvoRequest>())).ThrowsAsync(new NotPartOfConversationException());
        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(convoRequest), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task CreateConversation_TooManyParticipants()
    {
        var participants = new List<string> { "user1", "user2", "user3" };
        var request = new CreateConvoRequest(participants, sendMessageRequest);
        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task CreateConversation_NotEnoughParticipants()
    {
        var participants = new List<string> { "user1"};
        var request = new CreateConvoRequest(participants, sendMessageRequest);
        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    //[Fact]
    //public async Task EnumerateConversations()
    //{
    //    conversationServiceMock.Setup(m => m.EnumerateConversations(username))
    //        .ReturnsAsync(new List<ConversationResponse>());

    //    var response = await httpClient.GetAsync($"/api/conversations/{username}");
    //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //}

    //[Fact]
    //public async Task EnumerateConversations_ProfileNotFound()
    //{
    //    conversationServiceMock.Setup(m => m.EnumerateConversations(username))
    //        .ThrowsAsync(new ProfileNotFoundException());

    //    var response = await httpClient.GetAsync($"/api/conversations/{username}");
    //    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    //}

    //[Fact]
    //public async Task EnumerateConversations_ConversationsNotFound()
    //{
    //    conversationServiceMock.Setup(m => m.EnumerateConversations(username))
    //        .ThrowsAsync(new ConversationNotFoundException());

    //    var response = await httpClient.GetAsync($"/api/conversations/{username}");
    //    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    //}
}
