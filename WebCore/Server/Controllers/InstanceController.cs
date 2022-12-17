using GlobalInfraction.WebCore.Server.Context;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstanceController : Controller
{
    private readonly SqliteDataContext _context;
    private readonly ILogger _logger;

    public InstanceController(SqliteDataContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateOrUpdate([FromBody] InstanceDto request)
    {
        // TODO: There should only be one instance per IP address. Check for this and return an error if it exists.
        var instance = await _context.Instances.FirstOrDefaultAsync(server => server.InstanceGuid == request.InstanceGuid);

        var requestIpAddress = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();

        if (instance is not null)
        {
            if (requestIpAddress is null || requestIpAddress != instance.InstanceIp)
            {
                _logger.LogWarning("{Instance} IP mismatch! Request: [{ReqIP}], Registered: [{InstanceIP}]",
                    instance.InstanceGuid, requestIpAddress, instance.InstanceIp);
            }

            return instance.Active
                ? StatusCode(202, "Instance exists, and is active.")
                : StatusCode(200, "Instance exists, but is not active.");
        }

        var ipAddress = requestIpAddress ?? request.InstanceIp;

        _context.Instances.Add(new EFInstance
        {
            InstanceGuid = request.InstanceGuid,
            InstanceIp = ipAddress,
            InstanceName = request.InstanceName,
            ApiKey = request.ApiKey,
            Active = false
        });

        await _context.SaveChangesAsync();

        return StatusCode(200, $"Instance added {request.InstanceGuid}");
    }

    [HttpGet("{guid}")]
    public async Task<ActionResult<InstanceDto>> GetServer(string guid)
    {
        var guidParse = Guid.TryParse(guid, out var guidResult);
        if (!guidParse) return BadRequest("Invalid guid");

        var result = await _context.Instances.FirstOrDefaultAsync(x => x.InstanceGuid == guidResult);
        if (result is null) return NotFound("Instance not found");

        return Ok(new InstanceDto
        {
            InstanceGuid = result.InstanceGuid,
            InstanceIp = result.InstanceIp,
            InstanceName = result.InstanceName,
            Active = result.Active
        });
    }
}
