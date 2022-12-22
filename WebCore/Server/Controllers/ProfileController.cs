using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Policy = "InstanceAPIKeyAuth")]
    [HttpPost]
    public async Task<ActionResult> CreateOrUpdate([FromBody] ProfileDto request)
    {
        // todo: fluent validation on request
        var user = await _context.Profiles
            .SingleOrDefaultAsync(user => user.ProfileIdentity == request.ProfileIdentity);

        // TODO: This check needs to be changed to reply if the instance doesn't have permission to upload
        if (user is not null)
        {
            var existingAlias = await _context.ProfileMetas
                .AnyAsync(profile =>
                    profile.UserName == request.ProfileMeta.UserName && profile.IpAddress == request.ProfileMeta.IpAddress);

            if (!existingAlias)
            {
                var meta = new EFAlias
                {
                    UserName = request.ProfileMeta.UserName,
                    IpAddress = request.ProfileMeta.IpAddress,
                    Changed = DateTimeOffset.UtcNow,
                    ProfileId = user.Id
                };

                user.Aliases.Add(meta);
                user.CurrentAlias = meta;
            }

            user.Heartbeat = DateTimeOffset.UtcNow;

            _context.Profiles.Update(user);
            await _context.SaveChangesAsync();
            return StatusCode(StatusCodes.Status204NoContent);
        }

        var alias = new EFAlias
        {
            UserName = request.ProfileMeta.UserName,
            IpAddress = request.ProfileMeta.IpAddress,
            Changed = DateTimeOffset.UtcNow
        };

        // Create the user
        var newProfile = new EFProfile
        {
            ProfileIdentity = request.ProfileIdentity,
            Reputation = 10,
            Aliases = new List<EFAlias>
            {
                alias
            },
            CurrentAlias = alias,
            Heartbeat = DateTimeOffset.UtcNow
        };

        _context.Profiles.Add(newProfile);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProfileDto>>> GetUsers()
    {
        var result = await _context.Profiles.ToListAsync();

        var profileRequest = result
            .Select(profile => new ProfileDto
            {
                ProfileIdentity = profile.ProfileIdentity,
                ProfileMeta = new ProfileMetaDto
                {
                    UserName = profile.Aliases.Last().UserName,
                    IpAddress = profile.Aliases.Last().IpAddress, //TODO: THIS NEEDS TO BE PRIVILEGED
                    Changed = profile.Aliases.Last().Changed
                }
            }).ToList();

        return Ok(profileRequest);
    }

    [HttpGet("{identity}")]
    public async Task<ActionResult<ProfileDto>> GetUser(string identity)
    {
        var profile = await _context.Profiles.Where(profile => profile.ProfileIdentity == identity)
            .Select(profile => new
            {
                ProfileIdentity = identity,
                profile.Reputation,
                ProfileMeta = new ProfileMetaDto
                {
                    UserName = profile.CurrentAlias.UserName,
                    IpAddress = profile.CurrentAlias.IpAddress, //TODO: THIS NEEDS TO BE PRIVILEGED
                    Changed = profile.CurrentAlias.Changed
                }
            }).FirstOrDefaultAsync();

        if (profile is null)
        {
            return NotFound();
        }

        var infractions = await _context.Infractions.Where(infraction => infraction.Target.ProfileIdentity == identity)
            .Select(infraction => new InfractionDto
            {
                InfractionType = infraction.InfractionType,
                InfractionScope = infraction.InfractionScope,
                InfractionGuid = infraction.InfractionGuid,
                Admin = new ProfileDto
                {
                    ProfileIdentity = infraction.Admin.ProfileIdentity,
                    ProfileMeta = new ProfileMetaDto
                    {
                        UserName = infraction.Admin.CurrentAlias.UserName,
                        IpAddress = infraction.Admin.CurrentAlias.IpAddress,
                        Changed = infraction.Admin.CurrentAlias.Changed
                    },
                    Reputation = infraction.Admin.Reputation
                },
                Reason = infraction.Reason,
                Evidence = infraction.Evidence,
            })
            .ToListAsync();

        return Ok(new ProfileDto
        {
            ProfileIdentity = profile.ProfileIdentity,
            Reputation = profile.Reputation,
            ProfileMeta = profile.ProfileMeta,
            Infractions = infractions
        });
    }
}
