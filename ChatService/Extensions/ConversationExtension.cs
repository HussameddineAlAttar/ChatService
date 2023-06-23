using ChatService.DTO;
using ChatService.Exceptions;

namespace ChatService.Extensions;

public static class ConversationExtension
{
    public static bool CheckSenderValidity(this Conversation conversation, string sender)
    {
        var participants = conversation.Participants;
        if(!participants.Contains(sender))
        {
            return false;
        }
        return true;
    }
}