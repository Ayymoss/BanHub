using System.Security.Claims;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHub.WebCore.Shared.Models.PenaltiesView;
using BanHubData.Commands.Penalty;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PenaltyController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly SignedInUsers _signedInUsers;

    public PenaltyController(IMediator mediator, SignedInUsers signedInUsers)
    {
        _mediator = mediator;
        _signedInUsers = signedInUsers;
    }

    [HttpPost, PluginAuthentication]
    public async Task<IActionResult> AddPlayerPenaltyAsync([FromQuery] string authToken, [FromBody] AddPlayerPenaltyCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Item1 switch
        {
            ControllerEnums.ReturnState.Created => Ok(result.Item2.HasValue ? result.Item2.Value : "Error"),
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.Ok => Ok(),
            _ => BadRequest()
        };
    }

    [HttpPatch("Evidence"), PluginAuthentication]
    public async Task<IActionResult> AddPlayerPenaltyEvidenceAsync([FromQuery] string authToken,
        [FromBody] AddPlayerPenaltyEvidenceCommand request)
    {
        var result = await _mediator.Send(request);

        return result switch
        {
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.Conflict => Conflict(),
            _ => Ok()
        };
    }

    [HttpPost("Penalties")]
    public async Task<ActionResult<IEnumerable<Penalty>>> GetPenaltiesPaginationAsync([FromBody] GetPenaltiesPaginationCommand penaltiesPagination)
    {
        var result = await _mediator.Send(penaltiesPagination);
        return Ok(result);
    }

    [HttpGet("Penalties/{identity}")]
    public async Task<ActionResult<IEnumerable<Shared.Models.PlayerProfileView.Penalty>>> GetProfilePenaltiesAsync([FromQuery] string identity)
    {
        var result = await _mediator.Send(new GetProfilePenaltiesCommand {Identity = identity});
        return Ok(result);
    }

    [HttpGet("Latest")]
    public async Task<ActionResult<IEnumerable<Shared.Models.IndexView.Penalty>>> GetLatestBansAsync()
    {
        var result = await _mediator.Send(new GetLatestBansCommand());
        return Ok(result);
    }

    [HttpDelete("Delete")] // Authorised endpoint
    public async Task<IActionResult> RemovePenaltyAsync([FromBody] RemovePenaltyCommand request)
    {
        var adminUserName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        if (adminUserName is null || adminSignInGuid is null) return Unauthorized("You are not authorised to perform this action");
        
        var webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[]
        {
            WebRole.WebAdmin,
            WebRole.WebSuperAdmin
        });

        if (!webRoleAssigned) return Unauthorized("You are not authorised to perform this action");

        request.AdminUserName = adminUserName;
        var result = await _mediator.Send(request);
        return result switch
        {
            true => Ok(),
            false => BadRequest()
        };
    }
}
