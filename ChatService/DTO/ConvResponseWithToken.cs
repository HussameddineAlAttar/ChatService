namespace ChatService.DTO;

public record ConvResponseWithToken
{
    public ConvResponseWithToken(List<ConversationResponse> responses, string token)
    {
        this.responses = responses;
        continuationToken = token;
    }
    public List<ConversationResponse> responses { get; }
    public string continuationToken { get; }
}
