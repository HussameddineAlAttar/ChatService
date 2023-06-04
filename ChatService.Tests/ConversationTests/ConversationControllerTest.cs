using ChatService.DTO;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using ChatService.Exceptions;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;

namespace ChatService.Tests.ConversationTests;

public class ConversationControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly CreateConversationRequest convoRequest;
    private readonly Conversation conversation;
    private readonly EnumerateConversationsEntry enumConvoResponse1;
    private readonly EnumerateConversationsEntry enumConvoResponse2;
    private readonly List<EnumerateConversationsEntry> enumConvoResponseList;
    private readonly EnumerateConversationsResponse convoTokenResponse;

    private readonly SendMessageRequest sendMessageRequest;
    private readonly EnumerateMessagesEntry enumMessageResponse1;
    private readonly EnumerateMessagesEntry enumMessageResponse2;
    private readonly List<EnumerateMessagesEntry> enumMessageResponseList;
    private readonly EnumerateMessagesResponse messageTokenResponse;

    private readonly List<string> participants;
    private readonly string username;
    private readonly string email;

    private readonly HttpClient httpClient;
    private readonly Mock<IConversationService> conversationServiceMock = new();
    private readonly Mock<IMessageService> messageServiceMock = new();

    private readonly int defaultLimit = 10;
    private readonly long defaultLastSeen = 1;
    private readonly string? nullToken = null;
    private readonly string defaultToken = "randomToken";

    public ConversationControllerTest(WebApplicationFactory<Program> factory)
    {
        httpClient = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services => { services.AddSingleton(conversationServiceMock.Object); });
            builder.ConfigureTestServices(services => { services.AddSingleton(messageServiceMock.Object); });
        }).CreateClient();

        username = "Foo";
        email = "FooBar@email.com";
        participants = new List<string> { "Foo", "Bar" };
        sendMessageRequest = new(Guid.NewGuid().ToString(), username, Guid.NewGuid().ToString());
        conversation = new Conversation(participants);
        convoRequest = new CreateConversationRequest(participants, sendMessageRequest);

        enumConvoResponse1 = new(Guid.NewGuid().ToString(), 123, new Profile("FooBar", "FooBar@email.com", "Foo", "Bar"));
        enumConvoResponse2 = new(Guid.NewGuid().ToString(), 456, new Profile("FizzBuzz", "FizzBuzz@email.com", "Fizz", "Buzz"));
        enumConvoResponseList = new() { enumConvoResponse2, enumConvoResponse1 };
        convoTokenResponse = new(enumConvoResponseList, username, defaultLimit, defaultLastSeen, defaultToken);

        enumMessageResponse1 = new("Hello World", "Foo", 1000);
        enumMessageResponse2 = new("Bye World", "Bar", 2000);
        enumMessageResponseList = new() { enumMessageResponse2, enumMessageResponse1 };
        messageTokenResponse = new(enumMessageResponseList, conversation.Id, defaultLimit, defaultLastSeen, defaultToken);
    }

    private bool EqualMessageList(List<EnumerateMessagesEntry> list1, List<EnumerateMessagesEntry> list2)
    {
        return list1.SequenceEqual(list2);
    }

    private bool EqualConversationList(List<EnumerateConversationsEntry> list1, List<EnumerateConversationsEntry> list2)
    {
        return list1.SequenceEqual(list2);
    }


    private bool EqualConvoRequest(CreateConversationRequest request1, CreateConversationRequest request2)
    {
        bool equalParticipants = request1.Participants.SequenceEqual(request2.Participants);
        var message1 = request1.FirstMessage.message;
        var message2 = request2.FirstMessage.message;
        bool equalMessages = message1.Id == message2.Id
            && message1.SenderUsername == message2.SenderUsername
            && message1.Text == message2.Text;

        return equalParticipants && equalMessages;
    }

    [Fact]
    public async Task CreateConversation()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(It.Is<CreateConversationRequest>(request => EqualConvoRequest(request, convoRequest))))
            .Returns(Task.CompletedTask);
        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(convoRequest), Encoding.Default, "application/json"));
        var json = await httpResponse.Content.ReadAsStringAsync();
        var createConvoResponse = JsonConvert.DeserializeObject<CreateConversationResponse>(json);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);
        Assert.Equal(conversation.Id, createConvoResponse.Id);
    }

    [Fact]
    public async Task CreateConversation_Conflict()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(It.Is<CreateConversationRequest>(request => EqualConvoRequest(request, convoRequest))))
            .ThrowsAsync(new ConversationConflictException());
        messageServiceMock.Setup(m => m.EnumerateMessages(conversation.Id, defaultLimit, defaultLastSeen, nullToken)).ReturnsAsync((enumMessageResponseList, defaultToken));

        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(convoRequest), Encoding.Default, "application/json"));
        var json = await httpResponse.Content.ReadAsStringAsync();
        var messagesWithUri = JsonConvert.DeserializeObject<EnumerateMessagesResponse>(json);

        string expectedUri = $"/api/conversations/{conversation.Id}/messages?limit={defaultLimit}&lastSeenMessageTime={defaultLastSeen}&continuationToken={defaultToken}";

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
        Assert.Equal(expectedUri, messagesWithUri.NextUri);
        Assert.True(EqualMessageList(messageTokenResponse.Messages, messagesWithUri.Messages));
    }

    [Fact]
    public async Task CreateConversation_ProfileNotFoundException()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(It.Is<CreateConversationRequest>(request => EqualConvoRequest(request, convoRequest))))
            .ThrowsAsync(new ProfileNotFoundException(new List<string>() { "BarFoo", "BooFar" }));
        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(convoRequest), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
    }

    [Fact]
    public async Task CreateConversation_NotPartOfConversation()
    {
        conversationServiceMock.Setup(m => m.CreateConversation(It.Is<CreateConversationRequest>(request => EqualConvoRequest(request, convoRequest))))
            .ThrowsAsync(new NotPartOfConversationException());
        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(convoRequest), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task CreateConversation_TooManyParticipants()
    {
        var participants = new List<string> { "user1", "user2", "user3" };
        var request = new CreateConversationRequest(participants, sendMessageRequest);
        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task CreateConversation_NotEnoughParticipants()
    {
        var participants = new List<string> { "user1" };
        var request = new CreateConversationRequest(participants, sendMessageRequest);
        var httpResponse = await httpClient.PostAsync("/api/conversations",
            new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }

    [Fact]
    public async Task EnumerateConversations()
    {
        conversationServiceMock.Setup(m => m.EnumerateConversations(username, defaultLimit, defaultLastSeen, nullToken))
            .ReturnsAsync((enumConvoResponseList, defaultToken));

        var response = await httpClient.GetAsync($"/api/conversations?username={username}");
        var json = await response.Content.ReadAsStringAsync();
        var conversationsWithUri = JsonConvert.DeserializeObject<EnumerateConversationsResponse>(json);

        string expectedUri = $"/api/conversations?username={username}&limit={defaultLimit}&lastSeenConversationTime={defaultLastSeen}&continuationToken={defaultToken}";

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedUri, conversationsWithUri.NextUri);
        Assert.True(EqualConversationList(convoTokenResponse.Conversations, conversationsWithUri.Conversations));

    }

    [Fact]
    public async Task EnumerateConversations_ProfileNotFound()
    {
        conversationServiceMock.Setup(m => m.EnumerateConversations(username, defaultLimit, defaultLastSeen, nullToken))
            .ThrowsAsync(new ProfileNotFoundException());
        var response = await httpClient.GetAsync($"/api/conversations?username={username}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
