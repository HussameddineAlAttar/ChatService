using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using ChatService.Storage;
using Moq;


namespace ChatService.Tests.ConversationTests;

public class ConversationServiceTest
{
    private readonly Mock<IConversationStore> conversationStoreMock = new();
    private readonly Mock<IProfileStore> profileStoreMock = new();
    private readonly Mock<IMessagesStore> messageStoreMock = new();
    private readonly ConversationService conversationService;

    private readonly CreateConversationRequest convoRequest;
    private readonly Conversation conversation1;
    private readonly Conversation conversation2;
    private readonly List<Conversation> conversationList;
    private readonly List<EnumerateConversationsEntry> enumConversationList;

    private readonly SendMessageRequest sendMessageRequest;
    private readonly List<string> participants1;
    private readonly List<string> participants2;
    private readonly Message message;
    private readonly string username;

    private readonly Profile FooProfile;
    private readonly Profile BarProfile;
    private readonly Profile NewBarProfile;

    private readonly int defaultLimit = 10;
    private readonly long defaultLastSeen = 1;
    private readonly string? nullToken = null;
    private readonly string defaultToken = "randomToken";

    public ConversationServiceTest()
    {
        conversationService = new ConversationService(conversationStoreMock.Object, profileStoreMock.Object, messageStoreMock.Object);
        username = "Foo";
        FooProfile = new Profile(username, "FirstName", "LastName", Guid.NewGuid().ToString());
        BarProfile = new Profile("Bar", "FirstName", "LastName", Guid.NewGuid().ToString());
        NewBarProfile = new Profile("NewBar", "FirstName", "LastName", Guid.NewGuid().ToString());

        sendMessageRequest = new(Guid.NewGuid().ToString(), username, "Hello World");
        message = sendMessageRequest.message;

        participants1 = new List<string> { "Foo", "Bar" };
        participants2 = new List<string> { "Foo", "NewBar" };
        conversation1 = new Conversation(participants1);
        conversation2 = new Conversation(participants2);
        conversationList = new() { conversation2, conversation1 };
        enumConversationList = new()
        {
            new EnumerateConversationsEntry(conversation2.Id, conversation2.ModifiedTime, NewBarProfile),
            new EnumerateConversationsEntry(conversation1.Id, conversation1.ModifiedTime, BarProfile)
        };

        convoRequest = new CreateConversationRequest(participants1, sendMessageRequest);
    }

    private bool EqualConversationList(List<EnumerateConversationsEntry> list1, List<EnumerateConversationsEntry> list2)
    {
        for (int i = 0; i < list1.Count; ++i)
        {
            if (list1[i].Id != list2[i].Id || list1[i].Recipient != list2[i].Recipient)
            {
                return false;
            }
        }
        return true;
    }

    private bool EqualConversation(Conversation conv1, Conversation conv2)
    {
        return conv1.Participants.SequenceEqual(conv2.Participants) && conv1.Id == conv2.Id;
    }

    [Fact]
    public async Task CreateConversation()
    {
        messageStoreMock.Setup(x => x.SendMessage(conversation1.Id, message)).Returns(Task.CompletedTask);
        await conversationService.CreateConversation(convoRequest);

        messageStoreMock.Verify(x => x.SendMessage(conversation1.Id, message), Times.Once);
        conversationStoreMock.Verify(x => x.CreateConversation(It.Is<Conversation>(conv => EqualConversation(conv, conversation1))), Times.Once);
    }

    [Fact]
    public async Task CreateConversation_ProfileNotFound()
    {
        profileStoreMock.Setup(x => x.GetProfile(username)).ThrowsAsync(new ProfileNotFoundException());
        await Assert.ThrowsAsync<ProfileNotFoundException>(async () =>
        {
            await conversationService.CreateConversation(convoRequest);
        });
        messageStoreMock.Verify(x => x.SendMessage(conversation1.Id, message), Times.Never);
    }

    [Fact]
    public async Task CreateConversation_Conflict()
    {
        conversationStoreMock.Setup(x => x.CreateConversation(It.Is<Conversation>(conv => EqualConversation(conv, conversation1))))
            .ThrowsAsync(new ConversationConflictException());
        await Assert.ThrowsAsync<ConversationConflictException>(async () =>
        {
            await conversationService.CreateConversation(convoRequest);
        });
        messageStoreMock.Verify(x => x.SendMessage(conversation1.Id, message), Times.Once);
        conversationStoreMock.Verify(x => x.CreateConversation(It.Is<Conversation>(conv => EqualConversation(conv, conversation1))), Times.Once);
    }

    //[Fact]
    //public async Task EnumerateConversations()
    //{
    //    profileStoreMock.Setup(x => x.GetProfile(username)).ReturnsAsync(FooProfile);
    //    profileStoreMock.Setup(x => x.GetProfile("Bar")).ReturnsAsync(BarProfile);
    //    profileStoreMock.Setup(x => x.GetProfile("NewBar")).ReturnsAsync(NewBarProfile);

    //    conversationStoreMock.Setup(x => x.EnumerateConversations(username, defaultLimit, defaultLastSeen, nullToken))
    //        .ReturnsAsync((conversationList, defaultToken));
    //    string expectedUri = $"/api/conversations?username={username}&limit={defaultLimit}&lastSeenConversationTime={defaultLastSeen}&continuationToken={defaultToken}";
    //    var conversationTokenResponse = await conversationService.EnumerateConversations(username, defaultLimit, defaultLastSeen, nullToken);

    //    Assert.Equal(expectedUri, conversationTokenResponse.NextUri);
    //    Assert.True(EqualConversationList(enumConversationList, conversationTokenResponse.Conversations));
    //}

    //[Fact]
    //public async Task EnumerateConversations_NoConversations()
    //{
    //    profileStoreMock.Setup(x => x.GetProfile(username)).ReturnsAsync(new Profile(username, "first", "last"));
    //    conversationStoreMock.Setup(x => x.EnumerateConversations(username, defaultLimit, defaultLastSeen, nullToken))
    //        .ReturnsAsync((new List<Conversation>() { }, nullToken));
    //    var conversationTokenResponse = await conversationService.EnumerateConversations(username, defaultLimit, defaultLastSeen, nullToken);

    //    Assert.True(string.IsNullOrWhiteSpace(conversationTokenResponse.NextUri));
    //    Assert.Empty(conversationTokenResponse.Conversations);
    //}

    [Fact]
    public async Task EnumerateConversations_UserNotFound()
    {
        profileStoreMock.Setup(x => x.GetProfile(username)).ThrowsAsync(new ProfileNotFoundException());
        await Assert.ThrowsAsync<ProfileNotFoundException>(async () =>
        {
            await conversationService.EnumerateConversations(username);
        });
        conversationStoreMock.Verify(x => x.EnumerateConversations(username, It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ModifyTime()
    {
        conversationStoreMock.Setup(x => x.UpdateLastModifiedTime(conversation1.Id, participants1, 123)).Returns(Task.CompletedTask);
        var time = await conversationService.UpdateLastModifiedTime(conversation1.Id, 123);
        Assert.Equal(123, time);
    }

    [Fact]
    public async Task ModifyTime_ConversationNotFound()
    {
        conversationStoreMock.Setup(x => x.UpdateLastModifiedTime(conversation1.Id, participants1, It.IsAny<long>())).ThrowsAsync(new ConversationNotFoundException());
        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
        {
            await conversationService.UpdateLastModifiedTime(conversation1.Id, 123);
        });
    }
}