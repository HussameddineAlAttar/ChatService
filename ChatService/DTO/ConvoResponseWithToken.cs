using System.Net;

namespace ChatService.DTO;

public record ConvoResponseWithToken
{
    public ConvoResponseWithToken(List<EnumConvoResponse> Conversations,
        string username, int? limit, long? lastSeenConversationTime, string? continuationToken)
    {
        this.Conversations = Conversations;
        encodedToken = WebUtility.UrlEncode(continuationToken);
        if (continuationToken == null)
        {
            NextUri = "";
        }
        else
        {
            NextUri = $"/api/conversations?username={username}&limit={limit}&lastSeenConversationTime={lastSeenConversationTime}&continuationToken={encodedToken}";
        }
    }
    public List<EnumConvoResponse> Conversations { get; }
    public string? encodedToken;
    public string NextUri { get; set; }
}
