using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using ChatService.Storage.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers;

[ApiController]
[Route("conversations/{conversationId}/messages")]
public class MessageController : ControllerBase
{
    private readonly IMessageService messageService;

    public MessageController(IMessageService _messageService)
    {
        messageService = _messageService;
    }

    [HttpPost]
    public async Task<ActionResult<long>> SendMessage(string conversationId, SendMessageRequest request)
    {
        Message message = request.message;
        try
        {
            var time = await messageService.SendMessage(conversationId, message);
            return CreatedAtAction(nameof(SendMessage), new { CreatedUnixTime = time}, time);
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
    public async Task<ActionResult<List<EnumMessageResponse>>> EnumurateMessages(string conversationId)
    {
        try
        {
            var messages = await messageService.EnumerateMessages(conversationId);
            return Ok(messages);
        }
        catch (Exception e)
        {
            if (e is ConversationNotFoundException)
            {
                return NotFound($"Conversation with id {conversationId} not found");
            }
            throw;
        }
    }

    [HttpGet("test")]
    public async Task<ActionResult<List<MessageTokenResponse>>> GetMessages(string conversationId, int limit = 10, long? lastSeenMessageTime = 1, string? continuationToken = null)
    {
        try
        {
            var messages = await messageService.GetMessages(conversationId, limit, lastSeenMessageTime, continuationToken);
            return Ok(messages);
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
