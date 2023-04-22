using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ChatService.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationController : ControllerBase
{
    private readonly IMessageService messageService;
    private readonly IConversationService conversationService;

    public ConversationController(IConversationService _conversationService, IMessageService _messageService)
    {
        conversationService = _conversationService;
        messageService = _messageService;
    }

    [HttpPost]
    public async Task<ActionResult<CreateConvoResponse>> CreateConversation(CreateConvoRequest request)
    {
        var conversation = new Conversation(request.Participants);
        var count = request.Participants.Count;
        if (count < 2)
        {
            return BadRequest($"Not enough participants to create conversation: {2 - count} more needed.");
        }
        else if (count > 2)
        {
            return BadRequest($"Too many participants: {count} present when maximum supported is 2.");
        }
        try
        {
            await conversationService.CreateConversation(request);
            var response = new CreateConvoResponse(conversation.Id, conversation.CreatedTime);
            return CreatedAtAction(nameof(CreateConversation), response);
        }
        catch (Exception e)
        {
            if(e is ConversationConflictException)
            {
                var messages = await messageService.EnumerateMessages(conversation.Id);
                return Ok(messages);
            }
            if (e is ProfileNotFoundException notFoundException)
            {
                return NotFound($"Cannot create conversation between non-existing users: {string.Join(", ", notFoundException.Usernames)}");
            }
            if(e is NotPartOfConversationException)
            {
                return BadRequest($"User {request.FirstMessage.SenderUsername} is not part the conversation.");
            }
            throw;
        }
    }

    [HttpGet]
    public async Task<ActionResult<ConvoResponseWithToken>> EnumerateConversations(string username, int limit = 10, long? lastSeenConversationTime = 1, string? continuationToken = null)
    {
        try
        {
            var responseWithUri = await conversationService.EnumerateConversations(username, limit, lastSeenConversationTime, WebUtility.UrlEncode(continuationToken));
            return Ok(responseWithUri);
        }
        catch (Exception e)
        {
            if (e is ProfileNotFoundException)
            {
                return NotFound($"Profile with username {username} not found");
            }
            throw;
        }
    }
}
