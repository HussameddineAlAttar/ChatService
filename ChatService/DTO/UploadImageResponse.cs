namespace ChatService.DTO;

public class UploadImageResponse
{
    public string Id { get; set; }

    public UploadImageResponse(string id)
    {
        Id = id;
    }
}