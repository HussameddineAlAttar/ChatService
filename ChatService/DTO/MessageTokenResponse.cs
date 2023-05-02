using System.Net;

namespace ChatService.DTO;

public record MessageTokenResponse
{
    public MessageTokenResponse(List<EnumMessageResponse> Messages, string conversationId,
        int? limit = 10, long? lastSeenMessageTime = 1, string? continuationToken = null)
    {
        this.Messages = Messages;
        encodedToken = WebUtility.UrlEncode(continuationToken);
        if(continuationToken == null)
        {
            NextUri = "";
        }
        else
        {
            NextUri = $"/api/conversations/{conversationId}/messages?limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={encodedToken}";
        }
    }
    public List<EnumMessageResponse> Messages { get; }
    public string? encodedToken;
    public string NextUri { get; set; }
}