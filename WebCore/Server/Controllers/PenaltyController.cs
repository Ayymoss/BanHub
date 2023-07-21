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
            ControllerEnums.ReturnState.Conflict => Conflict(),
            _ => BadRequest()
        };
    }

    [HttpPatch("Evidence"), PluginAuthentication]
    public async Task<IActionResult> AddPlayerPenaltyEvidenceAsync([FromQuery] string authToken,
        [FromBody] AddPlayerPenaltyEvidenceCommand request) =>
        await HandleAddPlayerPenaltyEvidence(request);

    [HttpPatch("WebEvidence")]
    public async Task<IActionResult> AddPlayerPenaltyWebEvidenceAsync([FromBody] AddPlayerPenaltyEvidenceCommand request)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var adminName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var adminNameIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (adminSignInGuid is null || adminName is null || adminNameIdentity is null)
            return Unauthorized("You are not authorised to perform this action");

        var instanceRoleAssigned = _signedInUsers.IsUserInAnyCommunityRole(adminSignInGuid, new[]
        {
            CommunityRole.Moderator,
            CommunityRole.Administrator,
            CommunityRole.SeniorAdmin,
            CommunityRole.Owner
        });
        var webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[]
        {
            WebRole.Admin,
            WebRole.SuperAdmin
        });

        if (!instanceRoleAssigned && !webRoleAssigned) return Unauthorized("You are not authorised to perform this action");

        request.IssuerUsername = adminName;
        request.IssuerIdentity = adminNameIdentity;
        return await HandleAddPlayerPenaltyEvidence(request);
    }

    [HttpPost("Pagination")]
    public async Task<ActionResult<IEnumerable<Penalty>>> GetPenaltiesPaginationAsync(
        [FromBody] GetPenaltiesPaginationCommand penaltiesPagination)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var instanceRoleAssigned = false;
        var webRoleAssigned = false;

        if (adminSignInGuid is not null)
        {
            instanceRoleAssigned = _signedInUsers.IsUserInAnyCommunityRole(adminSignInGuid, new[]
            {
                CommunityRole.Moderator,
                CommunityRole.Administrator,
                CommunityRole.SeniorAdmin,
                CommunityRole.Owner
            });
            webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[]
            {
                WebRole.Admin,
                WebRole.SuperAdmin
            });
        }

        penaltiesPagination.Privileged = instanceRoleAssigned || webRoleAssigned;
        var result = await _mediator.Send(penaltiesPagination);
        return Ok(result);
    }

    [HttpGet("Profile/Penalties/{identity}")]
    public async Task<ActionResult<IEnumerable<Shared.Models.PlayerProfileView.Penalty>>> GetProfilePenaltiesAsync(
        [FromRoute] string identity)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var instanceRoleAssigned = false;
        var webRoleAssigned = false;

        if (adminSignInGuid is not null)
        {
            instanceRoleAssigned = _signedInUsers.IsUserInAnyCommunityRole(adminSignInGuid, new[]
            {
                CommunityRole.Moderator,
                CommunityRole.Administrator,
                CommunityRole.SeniorAdmin,
                CommunityRole.Owner
            });
            webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[]
            {
                WebRole.Admin,
                WebRole.SuperAdmin
            });
        }

        var result = await _mediator.Send(new GetProfilePenaltiesCommand
        {
            Identity = identity,
            Privileged = instanceRoleAssigned || webRoleAssigned
        });
        return Ok(result);
    }

    [HttpGet("Community/Penalties/{identity}")]
    public async Task<ActionResult<IEnumerable<Shared.Models.PlayerProfileView.Penalty>>> GetCommunityPenaltiesAsync(
        [FromRoute] string identity)
    {
        if (!Guid.TryParse(identity, out var guid)) return BadRequest();

        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var communityRoleAssigned = false;
        var webRoleAssigned = false;

        if (adminSignInGuid is not null)
        {
            communityRoleAssigned = _signedInUsers.IsUserInAnyCommunityRole(adminSignInGuid, new[]
            {
                CommunityRole.Moderator,
                CommunityRole.Administrator,
                CommunityRole.SeniorAdmin,
                CommunityRole.Owner
            });
            webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[]
            {
                WebRole.Admin,
                WebRole.SuperAdmin
            });
        }

        var result = await _mediator.Send(new GetCommunityPenaltiesCommand
        {
            Identity = guid,
            Privileged = communityRoleAssigned || webRoleAssigned
        });
        return Ok(result);
    }

    [HttpGet("Latest")]
    public async Task<ActionResult<IEnumerable<Shared.Models.IndexView.Penalty>>> GetLatestBansAsync()
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var instanceRoleAssigned = false;
        var webRoleAssigned = false;

        if (adminSignInGuid is not null)
        {
            instanceRoleAssigned = _signedInUsers.IsUserInAnyCommunityRole(adminSignInGuid, new[]
            {
                CommunityRole.Moderator,
                CommunityRole.Administrator,
                CommunityRole.SeniorAdmin,
                CommunityRole.Owner
            });
            webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[]
            {
                WebRole.Admin,
                WebRole.SuperAdmin
            });
        }

        var result = await _mediator.Send(new GetLatestBansCommand
        {
            Privileged = instanceRoleAssigned || webRoleAssigned
        });
        return Ok(result);
    }

    [HttpDelete("Delete")] // Authorised endpoint
    public async Task<IActionResult> RemovePenaltyAsync([FromBody] RemovePenaltyCommand request)
    {
        var adminSignInGuid = User.Claims.FirstOrDefault(c => c.Type == "SignedInGuid")?.Value;
        var adminName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var adminNameIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (adminName is null || adminSignInGuid is null || adminNameIdentity is null)
            return Unauthorized("You are not authorised to perform this action");

        var webRoleAssigned = _signedInUsers.IsUserInAnyWebRole(adminSignInGuid, new[]
        {
            WebRole.Admin,
            WebRole.SuperAdmin
        });

        if (!webRoleAssigned) return Unauthorized("You are not authorised to perform this action");

        request.ActionAdminUserName = adminName;
        request.ActionAdminIdentity = adminNameIdentity;
        var result = await _mediator.Send(request);
        if (!result) return BadRequest();
        return Ok();
    }

    // Inline handler
    private async Task<IActionResult> HandleAddPlayerPenaltyEvidence(AddPlayerPenaltyEvidenceCommand request)
    {
        var result = await _mediator.Send(request);

        return result switch
        {
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.Conflict => Conflict(),
            _ => Ok()
        };
    }
}
