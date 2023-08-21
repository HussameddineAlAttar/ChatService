using Microsoft.AspNetCore.Mvc;
using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using ChatService.Extensions;

namespace ChatService.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileStore profileStore;
    private readonly ILogger<ProfileController> logger;
    private readonly TelemetryClient telemetryClient;

    public ProfileController(IProfileStore _profileStore, ILogger<ProfileController> _logger, TelemetryClient _telemetryClient)
    {
        profileStore = _profileStore;
        logger = _logger;
        telemetryClient = _telemetryClient;
    }

    private async Task CheckUniqueEmail(string email)
    {
        try
        {
            await profileStore.GetProfileByEmail(email);
            throw new ProfileConflictException($"Email {email} is taken.");
        }
        catch (Exception e)
        {
            if (e is ProfileNotFoundException)
            {
                return;
            }
            throw;
        }
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
        using (logger.BeginScope("{Username}", username))
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var profile = await profileStore.GetProfile(username);
                telemetryClient.TrackMetric("ProfileStore.GetProfile.Time", stopwatch.ElapsedMilliseconds);
                return Ok(profile);
            }
            catch (Exception e)
            {
                if (e is ProfileNotFoundException)
                {
                    return NotFound($"Profile with username {username} not found.");
                }
                throw;
            }
        }
    }

    [HttpPost]
    public async Task<ActionResult<Profile>> AddProfile(Profile profile)
    {
        if (!profile.Email.IsValidEmail())
        {
            return BadRequest("Email format is invalid");
        }
        using (logger.BeginScope("{Username}", profile.Username))
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await CheckUniqueEmail(profile.Email);

                profile.Password = profile.Password.BCryptHash();
                await profileStore.CreateProfile(profile);

                telemetryClient.TrackMetric("ProfileStore.AddProfile.Time", stopwatch.ElapsedMilliseconds);
                logger.LogInformation("Created a Profile");
                telemetryClient.TrackEvent("ProfileCreated");

                return CreatedAtAction(nameof(GetProfile), new { username = profile.Username }, profile);
            }
            catch (Exception e)
            {
                if (e is ProfileConflictException)
                {
                    return Conflict("Cannot create profile.\n" + e.Message);
                }
                throw;
            }
        }
    }
}
