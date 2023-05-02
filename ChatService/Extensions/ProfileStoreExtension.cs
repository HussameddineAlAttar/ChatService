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

    public static async Task<List<EnumConvoResponse>> GetProfilesOfParticipants(this IProfileStore store, string sender, List<Conversation> conversations)
    {
        List<EnumConvoResponse> response = new();
        foreach (var conversation in conversations)
        {
            Profile Recipient;
            if (conversation.Participants[0] != sender)
            {
                Recipient = await store.GetProfile(conversation.Participants[0]);
            }
            else
            {
                Recipient = await store.GetProfile(conversation.Participants[1]);
            }
            response.Add(new EnumConvoResponse(conversation.Id, conversation.ModifiedTime, Recipient));
        }
        return response;
    }
}
