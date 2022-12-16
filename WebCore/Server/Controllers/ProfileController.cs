using System.Text;
using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Enums;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : Controller
{
    private readonly SqliteDataContext _context;

    public ProfileController(SqliteDataContext context)
    {
        _context = context;
    }

    // TODO: test this method. It's returning http "The response ended prematurely."
    [HttpPost]
    public async Task<ActionResult<ProfileRequestModel>> CreateOrUpdate([FromBody] ProfileRequestModel request)
    {
        var user = await _context.Profiles.FirstOrDefaultAsync(user =>
            user.ProfileGame == request.ProfileGame && user.ProfileGuid == request.ProfileGuid);

        // Check if user exists
        if (user is not null)
        {
            var userName = user.ProfileMetas.FirstOrDefault(identity => identity.UserName == request.ProfileMeta.UserName);
            var ipAddress = user.ProfileMetas.FirstOrDefault(identity => identity.IpAddress == request.ProfileMeta.IpAddress);

            if (userName is null && ipAddress is null)
            {
                var meta = new EFProfileMeta
                {
                    UserName = request.ProfileMeta.UserName,
                    IpAddress = request.ProfileMeta.IpAddress,
                    Changed = DateTimeOffset.UtcNow,
                    UserId = user.Id
                };
                user.ProfileMetas.Add(meta);
            }

            user.LastConnected = DateTimeOffset.UtcNow;

            _context.Profiles.Update(user);
            await _context.SaveChangesAsync();
            
            return new ProfileRequestModel
            {
                ProfileGuid = user.ProfileGuid,
                ProfileGame = user.ProfileGame,
                ProfileMeta = request.ProfileMeta,
                Reputation = user.Reputation,
                Infractions = user.Infractions
                    .Where(x => x.Profile.ProfileGuid == user.ProfileGuid && x.Profile.ProfileGame == user.ProfileGame)
                    .Select(infraction => new InfractionRequestModel
                    {
                        InfractionType = infraction.InfractionType,
                        InfractionScope = infraction.InfractionScope,
                        InfractionGuid = infraction.InfractionGuid,
                        AdminGuid = infraction.AdminGuid,
                        AdminUserName = infraction.AdminUserName,
                        Reason = infraction.Reason,
                        Evidence = infraction.Evidence,
                    }).ToList()
            };
        }

        // Create the user
        var newProfile = new EFProfile
        {
            ProfileGuid = request.ProfileGuid,
            ProfileGame = request.ProfileGame,
            Reputation = 10,
            ProfileMetas = new List<EFProfileMeta>
            {
                new()
                {
                    UserName = request.ProfileMeta.UserName,
                    IpAddress = request.ProfileMeta.IpAddress,
                    Changed = DateTimeOffset.UtcNow
                }
            },
            Infractions = new List<EFInfraction>()
        };
        _context.Profiles.Add(newProfile);

        await _context.SaveChangesAsync();

        return new ProfileRequestModel
        {
            ProfileGuid = newProfile.ProfileGuid,
            ProfileGame = newProfile.ProfileGame, 
            Reputation = newProfile.Reputation,
            ProfileMeta = newProfile.ProfileMetas.Select(x => new ProfileMetaRequestModel
            {
                UserName = x.UserName,
                IpAddress = x.IpAddress,
                Changed = x.Changed
            }).LastOrDefault()!
           
        };
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProfileRequestModel>>> GetUsers()
    {
        var result = await _context.Profiles.ToListAsync();

        var profileRequest = result
            .Select(profile => new ProfileRequestModel
            {
                ProfileGuid = profile.ProfileGuid,
                ProfileGame = profile.ProfileGame,
                ProfileMeta = new ProfileMetaRequestModel
                {
                    UserName = profile.ProfileMetas.Last().UserName,
                    IpAddress = profile.ProfileMetas.Last().IpAddress, //TODO: THIS NEEDS TO BE PRIVILEGED
                    Changed = profile.ProfileMetas.Last().Changed
                }
            }).ToList();

        return Ok(profileRequest);
    }

    [HttpGet("{guid}&{game}")]
    public async Task<ActionResult<ProfileRequestModel>> GetUser(string guid, string game)
    {
        var result = await _context.Profiles.FirstOrDefaultAsync(user => user.ProfileGame == game && user.ProfileGuid == guid);
        if (result is null) return NotFound("No user found");
        return Ok(new ProfileRequestModel
        {
            ProfileGuid = result.ProfileGuid,
            ProfileGame = result.ProfileGame,
            ProfileMeta = new ProfileMetaRequestModel
            {
                UserName = result.ProfileMetas.Last().UserName,
                IpAddress = result.ProfileMetas.Last().IpAddress, //TODO: THIS NEEDS TO BE PRIVILEGED
                Changed = result.ProfileMetas.Last().Changed
            }
        });
    }
}
