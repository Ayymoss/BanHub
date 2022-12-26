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
    private readonly ILogger<InstanceController> _logger;

    public InstanceController(SqliteDataContext context, ILogger<InstanceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateOrUpdate([FromBody] InstanceDto request)
    {
        // TODO: There should only be one instance per IP address. Check for this and return an error if it exists.
        var instanceGuid = await _context.Instances
            .AsTracking()
            .FirstOrDefaultAsync(server => server.InstanceGuid == request.InstanceGuid);
        var instanceApi = await _context.Instances
            .FirstOrDefaultAsync(server => server.ApiKey == request.ApiKey);

        var requestIpAddress = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        var ipAddress = requestIpAddress ?? request.InstanceIp;

        // New instance
        if (instanceApi is null && instanceGuid is null)
        {
            _context.Instances.Add(new EFInstance
            {
                InstanceGuid = request.InstanceGuid,
                InstanceIp = ipAddress,
                InstanceName = request.InstanceName,
                ApiKey = request.ApiKey,
                Active = false,
                HeartBeat = DateTimeOffset.UtcNow
            });

            await _context.SaveChangesAsync();

            return StatusCode(200, $"Instance added {request.InstanceGuid}");
        }

        // Check existing record
        if (instanceGuid is null || instanceApi is null) return StatusCode(400, "GUID + API mismatch");
        if (instanceGuid.Id != instanceApi.Id) return StatusCode(409, "Instance already exists with this API key.");

        // Warn if IP address has changed... this really shouldn't happen.
        if (requestIpAddress is null || requestIpAddress != instanceGuid.InstanceIp)
        {
            _logger.LogWarning("{Instance} IP mismatch! Request: [{ReqIP}], Registered: [{InstanceIP}]",
                instanceGuid.InstanceGuid, requestIpAddress, instanceGuid.InstanceIp);
        }

        // Update existing record
        instanceGuid.HeartBeat = DateTimeOffset.UtcNow;
        instanceGuid.InstanceName = request.InstanceName;
        _context.Instances.Update(instanceGuid);
        await _context.SaveChangesAsync();

        return instanceGuid.Active
            ? StatusCode(202, "Instance exists, and is active.")
            : StatusCode(200, "Instance exists, but is not active.");
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
