//using ChatService.DTO;
//using ChatService.Exceptions;
//using ChatService.Storage;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ChatService.Tests.ConversationTests;

//public class CosmosConversationStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
//{
//    private readonly IConversationStore conversationStore;
//    private readonly string user1;
//    private readonly string user2;
//    private readonly string user_enum1;
//    private readonly string user_enum2;
//    private readonly string user_enum3;

//    private readonly List<string> participants;
//    private readonly List<string> participants_enum1;
//    private readonly List<string> participants_enum2;
//    private readonly List<string> participants_modify;

//    private readonly Conversation conversation;
//    private readonly Conversation conversation_conflict;
//    private readonly Conversation conversation_enum1;
//    private readonly Conversation conversation_enum2;
//    private readonly Conversation conversation_modify;

//    public CosmosConversationStoreTest(WebApplicationFactory<Program> factory)
//    {
//        conversationStore = factory.Services.GetRequiredService<IConversationStore>();
//        user1 = "A" + Guid.NewGuid().ToString();
//        user2 = "B" + Guid.NewGuid().ToString();
//        user_enum1 = "C" + Guid.NewGuid().ToString();
//        user_enum2 = "D" + Guid.NewGuid().ToString();
//        user_enum3 = "E" + Guid.NewGuid().ToString();

//        participants = new List<string>() { user1, user2 };
//        conversation = new Conversation(participants);
//        conversation_conflict = new Conversation(participants);

//        participants_enum1 = new List<string>() { user_enum1, user_enum2 };
//        participants_enum2 = new List<string> { user_enum1, user_enum3 };
//        conversation_enum1 = new Conversation(participants_enum1);
//        conversation_enum2 = new Conversation(participants_enum2);

//        participants_modify = new List<string> { user_enum2, user_enum3};
//        conversation_modify = new Conversation(participants_modify);
//    }

//    public Task InitializeAsync()
//    {
//        return Task.CompletedTask;
//    }

//    public async Task DisposeAsync()
//    {
//        await conversationStore.DeleteConversation(conversation.Id);
//        await conversationStore.DeleteConversation(conversation_conflict.Id);
//        await conversationStore.DeleteConversation(conversation_enum1.Id);
//        await conversationStore.DeleteConversation(conversation_enum2.Id);
//        await conversationStore.DeleteConversation(conversation_modify.Id);
//    }

//    public bool EqualConversations(Conversation conv1, Conversation conv2)
//    {
//        bool attributesEqual = (conv1.Id == conv2.Id
//                             && conv1.ModifiedTime == conv2.ModifiedTime
//                             && conv1.CreatedTime == conv2.CreatedTime
//                             && conv1.Participants.Count == conv2.Participants.Count);
//        bool participantsEqual = true;
//        for(int i = 0; i < conv1.Participants.Count; ++i)
//        {
//            if (conv1.Participants[i] != conv2.Participants[i])
//            {
//                participantsEqual = false;
//                break;
//            }
//        }
//        return attributesEqual && participantsEqual;
//    }

//    [Fact]
//    public async Task CreateConversation()
//    {
//        await conversationStore.CreateConversation(conversation);
//        var stored_conversation = await conversationStore.FindConversationById(conversation.Id);
//        Assert.True(EqualConversations(conversation, stored_conversation));
//    }

//    [Fact]
//    public async Task CreateConversation_Conflict()
//    {
//        await conversationStore.CreateConversation(conversation_conflict);
//        await Assert.ThrowsAsync<ConversationConflictException>(async () =>
//        {
//            await conversationStore.CreateConversation(conversation_conflict);
//        });
//    }

//    [Fact]
//    public async Task FindConversation_NotFound()
//    {
//        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
//        {
//            await conversationStore.FindConversationById(Guid.NewGuid().ToString());
//        });
//    }

//    [Fact]
//    public async Task EnumConversations()
//    {
//        List<Conversation> myConv = new() { conversation_enum1, conversation_enum2 };
//        await conversationStore.CreateConversation(conversation_enum1);
//        await conversationStore.CreateConversation(conversation_enum2);

//        var uploadedConversations = await conversationStore.EnumerateConversations(user_enum1);
//        Assert.Equal(myConv.Count, uploadedConversations.Count);
//        for(int i = 0; i < myConv.Count; i++)
//        {
//            Assert.True(EqualConversations(myConv[i], uploadedConversations[i]));
//        }
//    }

//    [Fact]
//    public async Task EnumConversations_NotFound()
//    {
//        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
//        {
//            await conversationStore.EnumerateConversations(Guid.NewGuid().ToString());
//        });
//    }

//    [Fact]
//    public async Task ModifyTime()
//    {
//        await conversationStore.CreateConversation(conversation_modify);
//        conversation_modify.ModifiedTime = 987;
//        await conversationStore.UpdateLastModifiedTime(conversation_modify.Id, 987);
//        var modifiedConversation = await conversationStore.FindConversationById(conversation_modify.Id);
//        Assert.True(EqualConversations(conversation_modify, modifiedConversation));
//    }

//    [Fact]
//    public async Task ModifyTime_NotFound()
//    {
//        await Assert.ThrowsAsync<ConversationNotFoundException>(async () =>
//        {
//            await conversationStore.UpdateLastModifiedTime(Guid.NewGuid().ToString(), 123);
//        });
//    }
//}
