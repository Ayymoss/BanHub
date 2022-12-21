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

    [HttpPost]
    public async Task<ActionResult<ProfileDto>> CreateOrUpdate([FromBody] ProfileDto request)
    {
        var user = await _context.Profiles
            .AsNoTracking()
            .Include(context => context.ProfileMetas)
            .Include(context => context.Infractions)
            .ThenInclude(x => x.Admin)
            .SingleOrDefaultAsync(user => user.ProfileIdentity == request.ProfileIdentity);


        // TODO: This check needs to be changed to reply if the instance doesn't have permission to upload
        // Check if user exists
        if (user is not null)
        {
            var userName = user.ProfileMetas.FirstOrDefault(profile => profile.UserName == request.ProfileMeta.UserName);
            var ipAddress = user.ProfileMetas.FirstOrDefault(profile => profile.IpAddress == request.ProfileMeta.IpAddress);

            if (userName is null || ipAddress is null)
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

            user.Heartbeat = DateTimeOffset.UtcNow;

            _context.Profiles.Update(user);
            await _context.SaveChangesAsync();

            var dtoReturn = new ProfileDto
            {
                ProfileIdentity = user.ProfileIdentity,
                ProfileMeta = new ProfileMetaDto
                {
                    UserName = user.ProfileMetas.Last().UserName,
                    IpAddress = user.ProfileMetas.Last().IpAddress, //TODO: THIS NEEDS TO BE PRIVILEGED
                    Changed = user.ProfileMetas.Last().Changed
                },
                Reputation = user.Reputation,
                Heartbeat = DateTimeOffset.UtcNow,
                Infractions = user.Infractions
                    .Where(x => x.Target.ProfileIdentity == user.ProfileIdentity) //TODO This is null!?
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
                                UserName = infraction.Admin.ProfileMetas.Last().UserName,
                                IpAddress = infraction.Admin.ProfileMetas.Last().IpAddress,
                                Changed = infraction.Admin.ProfileMetas.Last().Changed
                            },
                            Reputation = infraction.Admin.Reputation
                        },
                        Reason = infraction.Reason,
                        Evidence = infraction.Evidence,
                    }).ToList()
            };

            return Ok(dtoReturn);
        }

        // Check to see if the instance is authorised to upload
        var instance = await _context.Instances
            .SingleOrDefaultAsync(instance => instance.ApiKey == request.Instance!.ApiKey);
        if (instance is null) return StatusCode(400, "Instance not found");
        if (!instance.Active) return StatusCode(401, "Server is not active");

        // Create the user
        var newProfile = new EFProfile
        {
            ProfileIdentity = request.ProfileIdentity,
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
            Infractions = new List<EFInfraction>(),
            Heartbeat = DateTimeOffset.UtcNow
        };
        _context.Profiles.Add(newProfile);

        await _context.SaveChangesAsync();

        return Ok(new ProfileDto
        {
            ProfileIdentity = newProfile.ProfileIdentity,
            Reputation = newProfile.Reputation,
            Heartbeat = DateTimeOffset.UtcNow,
            ProfileMeta = newProfile.ProfileMetas.Select(x => new ProfileMetaDto
            {
                UserName = x.UserName,
                IpAddress = x.IpAddress,
                Changed = x.Changed
            }).LastOrDefault()!
        });
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
                    UserName = profile.ProfileMetas.Last().UserName,
                    IpAddress = profile.ProfileMetas.Last().IpAddress, //TODO: THIS NEEDS TO BE PRIVILEGED
                    Changed = profile.ProfileMetas.Last().Changed
                }
            }).ToList();

        return Ok(profileRequest);
    }

    [HttpGet("{identity}")]
    public async Task<ActionResult<ProfileDto>> GetUser(string identity)
    {
        var profile = await _context.Profiles
            .Include(context => context.ProfileMetas)
            .Include(context => context.Infractions)
            .FirstOrDefaultAsync(profile => profile.ProfileIdentity == identity);

        if (profile is null) return NotFound("No user found");
        return Ok(new ProfileDto
        {
            ProfileIdentity = profile.ProfileIdentity,
            ProfileMeta = new ProfileMetaDto
            {
                UserName = profile.ProfileMetas.Last().UserName,
                IpAddress = profile.ProfileMetas.Last().IpAddress, //TODO: THIS NEEDS TO BE PRIVILEGED
                Changed = profile.ProfileMetas.Last().Changed
            },
            Infractions = profile.Infractions
                .Where(inf => inf.Target.ProfileIdentity == profile.ProfileIdentity)
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
                            UserName = infraction.Admin.ProfileMetas.Last().UserName,
                            IpAddress = infraction.Admin.ProfileMetas.Last().IpAddress,
                            Changed = infraction.Admin.ProfileMetas.Last().Changed
                        },
                        Reputation = infraction.Admin.Reputation
                    },
                    Reason = infraction.Reason,
                    Evidence = infraction.Evidence,
                }).ToList(),
            Reputation = profile.Reputation
        });
    }
}
