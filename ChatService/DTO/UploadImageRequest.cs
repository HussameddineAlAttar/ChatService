namespace ChatService.DTO;

public record UploadImageRequest(IFormFile File, string id);