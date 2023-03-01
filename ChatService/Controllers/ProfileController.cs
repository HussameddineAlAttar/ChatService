using Microsoft.AspNetCore.Mvc;
using ChatService.DTO;
using ChatService.Storage;

namespace ChatService.Controllers;


[ApiController]
[Route("profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileInterface profileInterface;
    private readonly IImageInterface blobStorage;

    public ProfileController(IProfileInterface _profileInterface, IImageInterface _imageInterface)
    {
        profileInterface = _profileInterface;
        blobStorage = _imageInterface;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
        var profile = await profileInterface.GetProfile(username);
        if(profile == null)
        {
            return NotFound($"Profile of username {username} was not found.");
        }
        return Ok(profile);
    }

    [HttpPost]
    public async Task<ActionResult<Profile>> AddProfile(Profile profile)
    {
        var existingProfile = await profileInterface.GetProfile(profile.Username);
        if(existingProfile != null) {
            return Conflict($"Cannot create profile. Username {profile.Username} is taken.");
        }
        var existingImage = await blobStorage.DownloadImage(profile.ProfilePictureId);
        if(existingImage == null)
        {
            return BadRequest($"Image with id {profile.ProfilePictureId} does not exist.");
        }
        await profileInterface.UpsertProfile(profile);
        return CreatedAtAction(nameof(GetProfile), new { username = profile.Username },profile);

    }

    //[HttpDelete("{username}")]
    //public async Task<ActionResult<Profile>> DeleteProfile(string username)
    //{
    //    var profile = await profileInterface.GetProfile(username);
    //    if (profile == null)
    //    {
    //        return NotFound($"Profile of username {username} was not found");
    //    }
    //    await profileInterface.DeleteProfile(username);
    //    return Ok($"Profile of username {username} successfully deleted");
    //}
}
