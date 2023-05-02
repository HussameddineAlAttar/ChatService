namespace ChatService.DTO;

public record CreateConversationRequest(List<string> Participants, SendMessageRequest FirstMessage);