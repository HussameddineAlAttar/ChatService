using ChatService.DTO;
using ChatService.Storage;

namespace ChatService.Extensions;

public static class ProfileStoreExtension
{
    public static async Task<List<string>> CheckForNonExistingProfile(this IProfileStore store, List<string> usernames)
    {
        List<Task> tasks = new();
        List<string> missingProfileUsernames = new();

        foreach (string username in usernames)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await store.GetProfile(username);
                }
                catch
                {
                    missingProfileUsernames.Add(username);
                }
            }));
        }
        await Task.WhenAll(tasks);
        return missingProfileUsernames;
    }

    public static async Task<List<ConversationResponse>> Conversation_to_ConversationResponse(this IProfileStore store, string username, List<Conversation> conversations)
    {
        List<ConversationResponse> response = new();
        for (int i = 0; i < conversations.Count; ++i)
        {
            List<Profile> profiles = new();
            for (int j = 0; j < conversations[i].Participants.Count; ++j)
            {
                if (conversations[i].Participants[j] == username)
                {
                    continue;
                }
                profiles.Add(await store.GetProfile(conversations[i].Participants[j]));
            }
            response.Add(new ConversationResponse(conversations[i].Id, conversations[i].ModifiedTime, profiles));
        }
        return response;
    }
}
