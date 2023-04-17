using ChatService.Storage;

namespace ChatService.Extensions;

public static class StringExtension
{
    public static List<string> SplitToUsernames(this string conversationId)
    {
        return conversationId.Split("_").ToList();
    }
}