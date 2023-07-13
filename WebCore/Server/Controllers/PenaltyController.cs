using System.Security.Claims;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Commands;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models;
using BanHub.WebCore.Shared.Models.PenaltiesView;
using BanHub.WebCore.Shared.Utilities;
using BanHub.WebCore.Shared.ViewModels;
using BanHubData.Commands.Penalty;
using BanHubData.Domains;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PenaltyController : ControllerBase
{
    private readonly IMediator _mediator;

    public PenaltyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost, PluginAuthentication]
    public async Task<IActionResult> AddPenalty([FromQuery] string authToken, [FromBody] AddPlayerPenaltyCommand request)
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

    [HttpPatch("SubmitEvidence"), PluginAuthentication]
    public async Task<IActionResult> SubmitEvidenceAsync([FromQuery] string authToken, [FromBody] AddPlayerPenaltyEvidenceCommand request)
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
    public async Task<ActionResult<IEnumerable<Shared.Models.PlayerProfileView.Penalty>>> GetProfilePenaltiesAsync(string identity)
    {
        var result = await _mediator.Send(new GetProfilePenaltiesCommand {Identity = identity});
        return Ok(result);
    }

    [HttpGet("Index")]
    public async Task<ActionResult<IEnumerable<PenaltyIndex>>> GetRecentPenalties()
    {
        var result = await _mediator.Send(new GetLatestBansCommand());
        return Ok(result);
    }

    [HttpDelete("Profile/Delete")] // Authorised endpoint
    public async Task<IActionResult> RemovePenaltyAsync([FromBody] RemovePenaltyCommand request)
    {
        var privileged = User.IsInAnyRole("WebAdmin", "WebSuperAdmin"); // TODO THIS IS REALLY INSECURE - GET FROM USER MANAGER
        var adminUserName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        if (!privileged || adminUserName is null) return Unauthorized("You are not authorised to perform this action");

        request.AdminUserName = adminUserName;
        var result = await _mediator.Send(request);
        return result switch
        {
            true => Ok(),
            false => BadRequest()
        };
    }
}
