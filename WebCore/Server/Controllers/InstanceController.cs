using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Commands.Instance;
using BanHubData.Commands.Instance;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates or Updates an instance.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateOrUpdateInstanceAsync([FromBody] CreateOrUpdateInstanceCommand request)
    {
        var headerIp = HttpContext.Request.Headers["X-Forwarded-For"]; // TODO Implement and test

        Console.WriteLine($"X-Forwarded-For: {headerIp}, InstanceIP: {request.InstanceIp}");
        request.HeaderIp = headerIp;
        var result = await _mediator.Send(request);

        return result switch
        {
            ControllerEnums.ReturnState.Created => StatusCode(StatusCodes.Status201Created), // New, added
            ControllerEnums.ReturnState.Conflict => Conflict(), // Conflicting GUIDs
            ControllerEnums.ReturnState.Accepted => Accepted(), // Activated
            ControllerEnums.ReturnState.Ok => Ok(), // Not activated
            _ => BadRequest() // Should never happen
        };
    }

    [HttpGet("Active")]
    public async Task<ActionResult> IsInstanceActiveAsync([FromQuery] string identity)
    {
        if (Guid.TryParse(identity, out var guid)) return BadRequest();
        var result = await _mediator.Send(new IsInstanceActiveCommand {InstanceGuid = guid});
        if (!result) return Unauthorized();
        return Accepted();
    }

    [HttpGet("{identity}")]
    public async Task<ActionResult<BanHub.WebCore.Shared.Models.InstanceProfileView.Instance>> GetInstance([FromQuery] string identity)
    {
        var result = await _mediator.Send(new GetInstanceCommand {InstanceGuid = identity});
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost("Instances")]
    public async Task<ActionResult<IEnumerable<Shared.Models.InstancesView.Instance>>> GetInstancesAsync([FromBody] GetInstancesPaginationCommand pagination)
    {
        var result = await _mediator.Send(pagination);
        return Ok(result);
    }
    
    [HttpGet("Profile/Servers/{identity}")]
    public async Task<ActionResult<IEnumerable<BanHub.WebCore.Shared.Models.InstanceProfileView.Instance>>> GetInstanceProfileServersAsync([FromQuery] string identity)
    {
        if (Guid.TryParse(identity, out var guid)) return BadRequest();
        var result = await _mediator.Send(new GetInstanceProfileServersCommand {Identity = guid});
        return Ok(result);
    } // 
}
