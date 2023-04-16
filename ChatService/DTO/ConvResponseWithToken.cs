using System.Net;

namespace ChatService.DTO;

public record ConvResponseWithToken
{
    public ConvResponseWithToken(List<ConversationResponse> Conversations,
        string username, int? limit, long? lastSeenConversationTime, string? continuationToken)
    {
        this.Conversations = Conversations;
        encodedToken = WebUtility.UrlEncode(continuationToken);
        NextUri = $"/api/conversations?username={username}&limit={limit}&lastSeenConversationTime={lastSeenConversationTime}&continuationToken={encodedToken}";
    }
    public List<ConversationResponse> Conversations { get; }
    public string? encodedToken;
    public string NextUri { get; set; }
}
