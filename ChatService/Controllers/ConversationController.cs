using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace ChatService.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationController : ControllerBase
{
    private readonly IMessageService messageService;
    private readonly IConversationService conversationService;
    private readonly ILogger<ConversationController> logger;
    private readonly TelemetryClient telemetryClient;

    public ConversationController(IConversationService _conversationService, IMessageService _messageService,
        ILogger<ConversationController> _logger, TelemetryClient _telemetryClient)
    {
        conversationService = _conversationService;
        messageService = _messageService;
        logger = _logger;
        telemetryClient = _telemetryClient;
    }

    [HttpPost]
    public async Task<ActionResult<CreateConversationResponse>> CreateConversation(CreateConversationRequest request)
    {
        var conversation = new Conversation(request.Participants);
        using (logger.BeginScope("{ConversationID}", conversation.Id))
        {
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
                var stopwatch = Stopwatch.StartNew();
                await conversationService.CreateConversation(request);

                telemetryClient.TrackMetric("ConversationStore.CreateConversation.Time", stopwatch.ElapsedMilliseconds);
                logger.LogInformation("Created a Conversation");
                telemetryClient.TrackEvent("ConversationCreated");

                var response = new CreateConversationResponse(conversation.Id, conversation.CreatedTime);
                return CreatedAtAction(nameof(CreateConversation), response);
            }
            catch (Exception e)
            {
                if (e is ConversationConflictException)
                {
                    var stopwatch = Stopwatch.StartNew();
                    (var messageResponses, var token) = await messageService.EnumerateMessages(conversation.Id);
                    telemetryClient.TrackMetric("MessageStore.GetMessages.Time", stopwatch.ElapsedMilliseconds);

                    var messageTokenResponse = new EnumerateMessagesResponse(messageResponses, conversation.Id, continuationToken: token);
                    return Ok(messageTokenResponse);
                }
                if (e is ProfileNotFoundException notFoundException)
                {
                    return NotFound($"Cannot create conversation between non-existing users: {string.Join(", ", notFoundException.Usernames)}");
                }
                if (e is NotPartOfConversationException)
                {
                    return BadRequest($"User {request.FirstMessage.SenderUsername} is not part the conversation.");
                }
                throw;
            }
        }
    }

    [HttpGet]
    public async Task<ActionResult<EnumerateConversationsResponse>> EnumerateConversations(string username, int limit = 10, long? lastSeenConversationTime = 1, string? continuationToken = null)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            (var convoResponses, string token) = await conversationService.EnumerateConversations(username, limit, lastSeenConversationTime, continuationToken);
            telemetryClient.TrackMetric("ConversationStore.GetConversations.Time", stopwatch.ElapsedMilliseconds);
            EnumerateConversationsResponse responseWithUri = new(convoResponses, username, limit, lastSeenConversationTime, token);
            return Ok(responseWithUri);
        }
        catch (ProfileNotFoundException)
        {
            return NotFound($"Profile with username {username} not found");
        }
    }
}
