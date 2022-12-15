using GlobalBan.WebCore.Server.Context;
using GlobalBan.WebCore.Server.Models;
using GlobalBan.WebCore.Shared.Enums;
using GlobalBan.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalBan.WebCore.Server.Controllers;

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
    public async Task<ActionResult<string>> AddInfraction([FromBody] InfractionRequestModel request)
    {
        // Get user from database
        var user = _context.Profiles
            .AsTracking()
            .FirstOrDefault(x => x.ProfileGuid == request.Profile.ProfileIdentifier);

        if (user is null) return StatusCode(404, "User not found");

        var instance = await _context.Instances
            .FirstOrDefaultAsync(x => x.InstanceGuid == request.Instance.InstanceGuid);

        if (instance is null) return StatusCode(500, "Server not found");

        var infractionModel = new EFInfraction
        {
            InfractionType = request.InfractionType,
            InfractionStatus = InfractionStatus.Active,
            InfractionScope = request.InfractionScope,
            InfractionGuid = Guid.NewGuid(),
            Submitted = DateTimeOffset.UtcNow,
            AdminGuid = request.AdminGuid,
            AdminUserName = request.AdminUserName,
            Reason = request.Reason,
            Evidence = request.Evidence,
            ServerId = instance.Id,
            UserId = user.Id
        };

        _context.Add(infractionModel);
        await _context.SaveChangesAsync();
        return Ok($"Ban added {request.Profile.ProfileIdentifier} {infractionModel.InfractionGuid}");
    }

    [HttpPatch]
    public async Task<ActionResult<string>> ExpireInfraction([FromBody] InfractionRequestModel request)
    {
        var infraction = await _context.Infractions
            .AsTracking()
            .FirstOrDefaultAsync(x => x.InfractionGuid == request.InfractionGuid);

        if (infraction is null) return StatusCode(404, "Infraction not found");

        infraction.InfractionStatus = InfractionStatus.Inactive;

        _context.Update(infraction);
        await _context.SaveChangesAsync();


        return Ok($"Infraction updated {request.Profile.ProfileIdentifier}");
    }
}
