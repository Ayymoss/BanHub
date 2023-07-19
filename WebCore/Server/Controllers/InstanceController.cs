using System.Security.Claims;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
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
    private readonly SignedInUsers _signedInUsers;

    public InstanceController(IMediator mediator, SignedInUsers signedInUsers)
    {
        _mediator = mediator;
        _signedInUsers = signedInUsers;
    }

    /// <summary>
    /// Creates or Updates an instance.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateOrUpdateInstanceAsync([FromBody] CreateOrUpdateInstanceCommand request)
    {
        var headerIp = HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var header)
            ? header.ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString();

        request.HeaderIp = headerIp;
        var result = await _mediator.Send(request);

        return result switch
        {
            ControllerEnums.ReturnState.Created => StatusCode(StatusCodes.Status201Created), // New, added
            ControllerEnums.ReturnState.Conflict => Conflict(), // Conflicting GUIDs
            ControllerEnums.ReturnState.Ok => Ok(), // Not activated
            _ => BadRequest() // Should never happen
        };
    }

    [HttpGet("Active/{identity}")]
    public async Task<ActionResult> IsInstanceActiveAsync([FromRoute] string identity)
    {
        if (!Guid.TryParse(identity, out var guid)) return BadRequest();
        var result = await _mediator.Send(new IsInstanceActiveCommand {InstanceGuid = guid});
        if (!result) return Unauthorized();
        return Accepted();
    }

    [HttpGet("{identity}")]
    public async Task<ActionResult<BanHub.WebCore.Shared.Models.InstanceProfileView.Instance>> GetInstance([FromRoute] string identity)
    {
        if (!Guid.TryParse(identity, out var guid)) return BadRequest();
        var result = await _mediator.Send(new GetInstanceCommand {InstanceGuid = guid});
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost("Pagination")]
    public async Task<ActionResult<IEnumerable<Shared.Models.InstancesView.Instance>>> GetInstancesAsync(
        [FromBody] GetInstancesPaginationCommand pagination)
    {
        var result = await _mediator.Send(pagination);
        return Ok(result);
    }

    [HttpGet("Profile/Servers/{identity}")]
    public async Task<ActionResult<IEnumerable<BanHub.WebCore.Shared.Models.InstanceProfileView.Instance>>> GetInstanceProfileServersAsync(
        [FromRoute] string identity)
    {
        if (!Guid.TryParse(identity, out var guid)) return BadRequest();
        var result = await _mediator.Send(new GetInstanceProfileServersCommand {Identity = guid});
        return Ok(result);
    }

    [HttpPatch("Activation/{identity}")]
    public async Task<IActionResult> ActivateInstanceAsync([FromRoute] string identity)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        if (adminSignInGuid is null) return Unauthorized("You are not authorised to perform this action");

        var webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[] {WebRole.WebSuperAdmin});
        if (!webRoleAssigned) return Unauthorized("You are not authorised to perform this action");

        if (!Guid.TryParse(identity, out var guid)) return BadRequest();
        var result = await _mediator.Send(new ToggleInstanceActivationCommand {InstanceGuid = guid});
        if (!result) return NotFound();
        return Ok();
    }
}
