using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace ChatService.Controllers;

[ApiController]
[Route("api/conversations/{conversationId}/messages")]
public class MessageController : ControllerBase
{
    private readonly IMessageService messageService;
    private readonly ILogger<MessageController> logger;

    public MessageController(IMessageService _messageService, ILogger<MessageController> _logger)
    {
        messageService = _messageService;
        logger = _logger;
    }

    [HttpPost]
    public async Task<ActionResult<SendMessageResponse>> SendMessage(string conversationId, SendMessageRequest request)
    {
        Message message = request.message;
        using (logger.BeginScope("{Username} {ConversationId}", message.SenderUsername, conversationId)) { 
            try
            {
                var time = await messageService.SendMessage(conversationId, message);
                var response = new SendMessageResponse(time);
                logger.LogInformation("Sent a Message");
                return CreatedAtAction(nameof(SendMessage), response);
            }
            catch (Exception e)
            {
                if (e is MessageConflictException)
                {
                    return Conflict($"Message with id {message.Id} already exists");
                }
                else if (e is ConversationNotFoundException)
                {
                    return NotFound($"Conversation with id {conversationId} not found");
                }
                else if (e is NotPartOfConversationException)
                {
                    return BadRequest($"User {message.SenderUsername} is not part of conversation {conversationId}.");
                }
                throw;
            }
        }
    }

    [HttpGet]
    public async Task<ActionResult<MessageTokenResponse>> EnumerateMessages(string conversationId, int limit = 10, long lastSeenMessageTime = 1, string? continuationToken = null)
    {
        using (logger.BeginScope("{ConversationId}", conversationId))
        {
            try
            {
                var messageTokenResponse = await messageService.EnumerateMessages(conversationId, limit, lastSeenMessageTime, continuationToken);
                logger.LogInformation("Retrieved Messages");
                return Ok(messageTokenResponse);
            }
            catch (Exception e)
            {
                if (e is ConversationNotFoundException)
                {
                    return NotFound($"Conversation with id {conversationId} not found.");
                }
                throw;
            }
        }
    }
}
