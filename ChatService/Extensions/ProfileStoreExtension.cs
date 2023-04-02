using ChatService.DTO;
using ChatService.Storage.Interfaces;

namespace ChatService.Extensions;

public static class ProfileStoreExtension
{
    public static async Task<List<string>> CheckFor_NonExistingProfile(this IProfileInterface store, List<string> usernames)
    {
        List<string> invalidProfiles = new();

        for (int i = 0; i < usernames.Count; i++)
        {
            try
            {
                await store.GetProfile(usernames[i]);
            }
            catch
            {
                invalidProfiles.Add(usernames[i]);
            }
        }
        return invalidProfiles;
    }
    public static async Task<List<ConversationResponse>> Conversation_to_ConversationResponse(this IProfileInterface store, string username, List<Conversation> conversations)
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
