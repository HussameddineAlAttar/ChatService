namespace ChatService.DTO;

public class MessageTokenResponse
{
    public MessageTokenResponse(List<EnumMessageResponse> responses, string token)
    {
        this.responses = responses;
        continuationToken = token;
    }
    public List<EnumMessageResponse> responses { get; }
    public string continuationToken { get; }
}
