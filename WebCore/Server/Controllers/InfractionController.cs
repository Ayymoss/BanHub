using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Enums;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfractionController : Controller
{
    private readonly SqliteDataContext _context;

    public InfractionController(SqliteDataContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<string>> AddInfraction([FromBody] InfractionDto request)
    {
        var user = await _context.Profiles
            .AsTracking()
            .FirstOrDefaultAsync(user => user.ProfileIdentity == request.Target.ProfileIdentity);

        var admin = await _context.Profiles.FirstOrDefaultAsync(profile => profile.ProfileIdentity == request.Admin.ProfileIdentity);
        if (user is null || admin is null) return StatusCode(404, "User not found");

        var infractionCheck = await _context.Infractions.FirstOrDefaultAsync(x => x.InfractionGuid == request.InfractionGuid);
        if (infractionCheck is not null) return StatusCode(409, "Infraction already exists");

        // TODO: Recursive ban. Find a way to prevent global bans triggering loads of kick infractions.

        var instance = await _context.Instances.FirstOrDefaultAsync(x => x.ApiKey == request.Instance.ApiKey);
        if (instance is null) return StatusCode(404, "Server not found");
        if (!instance.Active) return StatusCode(401, "Server is not active");

        var infractionModel = new EFInfraction
        {
            InfractionType = request.InfractionType,
            InfractionStatus = InfractionStatus.Active,
            InfractionScope = request.InfractionScope,
            InfractionGuid = request.InfractionGuid,
            Submitted = DateTimeOffset.UtcNow,
            AdminId = admin.Id,
            Reason = request.Reason,
            Evidence = request.Evidence,
            InstanceId = instance.Id,
            TargetId = user.Id
        };

        _context.Add(infractionModel);
        await _context.SaveChangesAsync();
        return Ok($"Ban added {request.Target.ProfileIdentity} {infractionModel.InfractionGuid}");
    }

    //[HttpPatch]
    //public async Task<ActionResult<string>> ExpireInfraction([FromBody] InfractionRequestModel request)
    //{
    //    var infraction = await _context.Infractions
    //        .AsTracking()
    //        .FirstOrDefaultAsync(x => x.InfractionGuid == request.InfractionGuid);

    //    if (infraction is null) return StatusCode(404, "Infraction not found");

    //    infraction.InfractionStatus = InfractionStatus.Inactive;

    //    _context.Update(infraction);
    //    await _context.SaveChangesAsync();

    //    return Ok($"Infraction updated {request.Profile.ProfileIdentifier}");
    //}
}
