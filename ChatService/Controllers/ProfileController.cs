﻿using Microsoft.AspNetCore.Mvc;
using ChatService.DTO;
using ChatService.Exceptions;
using ChatService.Storage;
using Microsoft.ApplicationInsights;

namespace ChatService.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileStore profileInterface;
    private readonly ILogger<ProfileController> logger;
    private readonly TelemetryClient telemetryClient;

    public ProfileController(IProfileStore _profileInterface, ILogger<ProfileController> _logger, TelemetryClient _telemetryClient)
    {
        profileInterface = _profileInterface;
        logger = _logger;
        telemetryClient = _telemetryClient;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
        using (logger.BeginScope("{Username}", username))
        {
            try
            {
                var profile = await profileInterface.GetProfile(username);
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
        using (logger.BeginScope("{Username}", profile.Username))
        {
            try
            {
                await profileInterface.CreateProfile(profile);
                logger.LogInformation("Created a Profile");
                telemetryClient.TrackEvent("ProfileCreated");
                return CreatedAtAction(nameof(GetProfile), new { username = profile.Username }, profile);
            }
            catch (Exception e)
            {
                if (e is ProfileConflictException)
                {
                    return Conflict($"Cannot create profile. Username {profile.Username} is taken.");
                }
                throw;
            }
        }
    }
}
