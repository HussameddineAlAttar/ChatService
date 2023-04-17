using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ChatService.Controllers;

[ApiController]
[Route("api/conversations/{conversationId}/messages")]
public class MessageController : ControllerBase
{
    private readonly IMessageService messageService;

    public MessageController(IMessageService _messageService)
    {
        messageService = _messageService;
    }

    [HttpPost]
    public async Task<ActionResult<SendMessageResponse>> SendMessage(string conversationId, SendMessageRequest request)
    {
        Message message = request.message;
        try
        {
            var time = await messageService.SendMessage(conversationId, message);
            var response = new SendMessageResponse(time);
            return CreatedAtAction(nameof(SendMessage), response);
        }
        catch (Exception e)
        {
            if(e is MessageConflictException)
            {
                return Conflict($"Message with id {message.Id} already exists");
            }
            else if(e is ConversationNotFoundException)
            {
                return NotFound($"Conversation with id {conversationId} not found");
            }
            else if(e is NotPartOfConversationException)
            {
                return BadRequest($"User {message.SenderUsername} is not part of conversation {conversationId}.");
            }
            throw;
        }
    }

    [HttpGet]
    public async Task<ActionResult<MessageTokenResponse>> EnumerateMessages(string conversationId, int limit = 10, long lastSeenMessageTime = 1, string? continuationToken = null)
    {
        try
        {
            var messageTokenResponse = await messageService.EnumerateMessages(conversationId, limit, lastSeenMessageTime, continuationToken);
            return Ok(messageTokenResponse);
        }
        catch (Exception e)
        {
            if (e is ConversationNotFoundException)
            {
                return NotFound($"Conversation with id {conversationId} not found.");
            }
            else if (e is MessageNotFoundException)
            {
                return NotFound($"Messages for conversation {conversationId} not found.");
            }
            throw;
        }
    }

}
