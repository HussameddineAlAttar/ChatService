namespace ChatService.DTO;

public record CreateConvoRequest(List<string> Participants, SendMessageRequest FirstMessage);