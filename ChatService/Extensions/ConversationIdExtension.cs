using ChatService.Storage;

namespace ChatService.Extensions;

public static class ConversationIdExtension
{
    public static List<string> SplitToUsernames(this string conversationId)
    {
        return conversationId.Split("_").ToList();
    }

    public static string JoinToConversationId(this List<string> usernames)
    {
        usernames.Sort();
        return string.Join("_", usernames);
    }
}