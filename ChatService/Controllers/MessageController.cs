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
            (var messages, var token) = await messageService.GetMessages(conversationId, limit, lastSeenMessageTime, continuationToken);
            var messageResponses = messages.Select(message =>
            new EnumMessageResponse(message.Text, message.SenderUsername, message.Time))
                .ToList();
            var messageTokenResponse = new MessageTokenResponse(messageResponses, token);
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
