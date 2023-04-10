using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage;

namespace ChatService.Extensions;

public static class ConversationStoreExtension
{
    public static async Task<bool> CheckIfConversationExists(this IConversationStore store, string conversationId)
    {
        try
        {
            await store.FindConversationById(conversationId);
            return true;
        }
        catch(Exception e)
        {
            if(e is ConversationNotFoundException)
            {
                return false;
            }
            throw;
        }
    }
}
