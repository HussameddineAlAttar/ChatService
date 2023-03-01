namespace ChatService.DTO;

public record UploadImageResponse
{
    public string imageId { get; set; }

    public UploadImageResponse(string id)
    {
        imageId = id;
    }
}