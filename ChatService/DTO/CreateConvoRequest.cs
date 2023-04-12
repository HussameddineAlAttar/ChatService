namespace ChatService.DTO;

public record CreateConvoRequest(Conversation Conversation, SendMessageRequest FirstMessageRequest);