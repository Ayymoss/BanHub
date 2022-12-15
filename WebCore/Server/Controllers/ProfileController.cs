using System.Text;
using GlobalBan.WebCore.Server.Context;
using GlobalBan.WebCore.Server.Models;
using GlobalBan.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalBan.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : Controller
{
    private readonly SqliteDataContext _context;

    public ProfileController(SqliteDataContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateOrUpdate([FromBody] ProfileRequestModel request)
    {
        var user = await _context.Profiles.FirstOrDefaultAsync(user => user.ProfileIdentifier == request.ProfileIdentifier);

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
            return Ok($"User updated {request.ProfileIdentifier}");
        }

        // TODO: test this method. It's returning http "The response ended prematurely."
        _context.Profiles.Add(new EFProfile
        {
            ProfileGuid = request.ProfileGuid,
            ProfileGame = request.ProfileGame,
            ProfileIdentifier = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Concat(request.ProfileGuid, request.ProfileGame))),
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
        });

        await _context.SaveChangesAsync();

        return Ok($"User added {request.ProfileGuid}");
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
                ProfileIdentifier = profile.ProfileIdentifier,
                ProfileMeta = new ProfileMetaRequestModel
                {
                    UserName = profile.ProfileMetas.Last().UserName,
                    IpAddress = profile.ProfileMetas.Last().IpAddress,
                    Changed = profile.ProfileMetas.Last().Changed
                }
            }).ToList();

        return Ok(profileRequest);
    }

    [HttpGet("{guid}")]
    public async Task<ActionResult<ProfileRequestModel>> GetUser(string guid)
    {
        var result = await _context.Profiles.FirstOrDefaultAsync(x => x.ProfileIdentifier == guid);
        if (result is null) return NotFound("No user found");
        return Ok(new ProfileRequestModel
        {
            ProfileGuid = result.ProfileGuid,
            ProfileGame = result.ProfileGame,
            ProfileIdentifier = result.ProfileIdentifier,
            ProfileMeta = new ProfileMetaRequestModel
            {
                UserName = result.ProfileMetas.Last().UserName,
                IpAddress = result.ProfileMetas.Last().IpAddress,
                Changed = result.ProfileMetas.Last().Changed
            }
        });
    }
}
