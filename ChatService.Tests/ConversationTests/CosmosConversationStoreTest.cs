using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Tests.ConversationTests;

public class CosmosConversationStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IConversationStore conversationStore;
    private readonly string user1;
    private readonly string user2;
    private readonly string user_enum1;
    private readonly string user_enum2;
    private readonly string user_enum3;

    private readonly List<string> participants;
    private readonly List<string> participants_enum1;
    private readonly List<string> participants_enum2;
    private readonly List<string> participants_modify;

    private readonly Conversation conversation;
    private readonly Conversation conversation_conflict;
    private readonly Conversation conversation_enum1;
    private readonly Conversation conversation_enum2;
    private readonly Conversation conversation_modify;

    private readonly int defaultLimit = 10;
    private readonly long defaultLastSeen = 1;
    private readonly string? nullToken = null;
    private readonly string defaultToken = "randomToken";

    public CosmosConversationStoreTest(WebApplicationFactory<Program> factory)
    {
        conversationStore = factory.Services.GetRequiredService<IConversationStore>();
        user1 = "A" + Guid.NewGuid().ToString();
        user2 = "B" + Guid.NewGuid().ToString();
        user_enum1 = "C" + Guid.NewGuid().ToString();
        user_enum2 = "D" + Guid.NewGuid().ToString();
        user_enum3 = "E" + Guid.NewGuid().ToString();

        participants = new List<string>() { user1, user2 };
        conversation = new Conversation(participants);
        conversation_conflict = new Conversation(participants);

        participants_enum1 = new List<string>() { user_enum1, user_enum2 };
        participants_enum2 = new List<string> { user_enum1, user_enum3 };
        conversation_enum1 = new Conversation(participants_enum1);
        conversation_enum2 = new Conversation(participants_enum2);

        participants_modify = new List<string> { user_enum2, user_enum3 };
        conversation_modify = new Conversation(participants_modify);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await conversationStore.DeleteConversation(conversation.Id);
        await conversationStore.DeleteConversation(conversation_conflict.Id);
        await conversationStore.DeleteConversation(conversation_enum1.Id);
        await conversationStore.DeleteConversation(conversation_enum2.Id);
        await conversationStore.DeleteConversation(conversation_modify.Id);
    }

    private bool EqualConversations(Conversation conv1, Conversation conv2)
    {
        bool attributesEqual = (conv1.Id == conv2.Id
                             && conv1.ModifiedTime == conv2.ModifiedTime
                             && conv1.CreatedTime == conv2.CreatedTime);
        bool participantsEqual = conv1.Participants.SequenceEqual(conv2.Participants);
        return attributesEqual && participantsEqual;
    }

    private bool EqualConversationLists(List<Conversation> list1, List<Conversation> list2)
    {
        if(list1.Count != list2.Count)
        {
            return false;
        }
        for(int i = 0; i < list1.Count; i++)
        {
            if (!EqualConversations(list1[i], list2[i]))
            {
                return false;
            }
        }
        return true;
    }

    [Fact]
    public async Task CreateConversation()
    {
        await conversationStore.CreateConversation(conversation);
        var stored_conversation = await conversationStore.FindConversationById(conversation.Id);
        Assert.True(EqualConversations(conversation, stored_conversation));
    }

    [Fact]
    public async Task CreateConversation_Conflict()
    {
        await conversationStore.CreateConversation(conversation_conflict);
        await Assert.ThrowsAsync<ConversationConflictException>(async () =>
        {
            await conversationStore.CreateConversation(conversation_conflict);
        });
    }

    [Fact]
    public async Task FindConversation_NotFound()
    {
        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
        {
            await conversationStore.FindConversationById(Guid.NewGuid().ToString());
        });
    }

    [Fact]
    public async Task EnumConversations()
    {
        List<Conversation> myConv = new() { conversation_enum2, conversation_enum1 };
        await conversationStore.CreateConversation(conversation_enum1);
        await conversationStore.CreateConversation(conversation_enum2);

        (var uploadedConversations, var token) = await conversationStore.EnumerateConversations(user_enum1, defaultLimit, defaultLastSeen, nullToken);
        Assert.True(EqualConversationLists(myConv, uploadedConversations));
    }

    [Fact]
    public async Task EnumConversations_NotFound()
    {
        (var uploadedConversations, var token) = await conversationStore.EnumerateConversations(Guid.NewGuid().ToString(), defaultLimit, defaultLastSeen, nullToken);
        Assert.Empty(uploadedConversations);
        Assert.True(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task ModifyTime()
    {
        await conversationStore.CreateConversation(conversation_modify);
        conversation_modify.ModifiedTime = 987;
        await conversationStore.UpdateLastModifiedTime(conversation_modify.Id, 987);
        var modifiedConversation = await conversationStore.FindConversationById(conversation_modify.Id);
        Assert.True(EqualConversations(conversation_modify, modifiedConversation));
    }

    [Fact]
    public async Task ModifyTime_NotFound()
    {
        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
        {
            await conversationStore.UpdateLastModifiedTime(Guid.NewGuid().ToString(), 123);
        });
    }
}
