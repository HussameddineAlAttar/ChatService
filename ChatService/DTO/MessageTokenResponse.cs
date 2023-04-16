using System.Net;

namespace ChatService.DTO;

public record MessageTokenResponse
{
    public MessageTokenResponse(List<EnumMessageResponse> Messages, string conversationId,
        int? limit, long? lastSeenMessageTime, string? continuationToken)
    {
        this.Messages = Messages;
        encodedToken = WebUtility.UrlEncode(continuationToken);
        NextUri = $"/api/conversations/{conversationId}/messages?limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={encodedToken}";
    }
    public List<EnumMessageResponse> Messages { get; }
    public string? encodedToken;
    public string NextUri { get; set; }
}