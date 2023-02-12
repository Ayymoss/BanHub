using System.Security.Claims;
using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Server.Utilities;
using BanHub.WebCore.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/v2/[controller]")]
public class PenaltyController : ControllerBase
{
    private readonly IPenaltyService _penaltyService;

    public PenaltyController(IPenaltyService penaltyService)
    {
        _penaltyService = penaltyService;
    }

    [HttpPost, PluginAuthentication]
    public async Task<ActionResult<string>> AddPenalty([FromQuery] string authToken, [FromBody] PenaltyDto request)
    {
        var result = await _penaltyService.AddPenalty(request);
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.Created => Ok(result.Item2.HasValue ? result.Item2.Value : "Error"),
            ControllerEnums.ProfileReturnState.NotFound => NotFound(),
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest(),
            ControllerEnums.ProfileReturnState.Conflict => Conflict("Infraction already exists"),
            ControllerEnums.ProfileReturnState.NoContent => NoContent(),
            _ => BadRequest() // Should never happen
        };
    }

    [HttpPost("Evidence"), PluginAuthentication]
    public async Task<ActionResult<bool>> SubmitEvidence([FromQuery] string authToken, [FromBody] PenaltyDto request)
    {
        var result = await _penaltyService.SubmitEvidence(request);

        return result switch
        {
            true => Ok(true),
            false => BadRequest(false)
        };
    }

    [HttpGet]
    public async Task<ActionResult<InstanceDto>> GetPenalty([FromQuery] string guid)
    {
        var result = await _penaltyService.GetPenalty(guid);
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.Ok => Ok(result.Item2),
            ControllerEnums.ProfileReturnState.NotFound => NotFound(),
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest(),
            _ => BadRequest() // Should never happen
        };
    }

    [HttpPost("All")]
    public async Task<ActionResult<IEnumerable<PenaltyDto>>> GetPenalties([FromBody] PaginationDto pagination)
    {
        return Ok(await _penaltyService.Pagination(pagination));
    }

    [HttpGet("Index")]
    public async Task<ActionResult<IEnumerable<PenaltyDto>>> GetRecentPenalties()
    {
        return Ok(await _penaltyService.GetLatestThreeBans());
    }

    [HttpPost("Remove")] // Authorised endpoint
    public async Task<ActionResult<bool>> RemovePenalty([FromBody] PenaltyDto request)
    {
        var privileged = User.IsInAnyRole("WebAdmin", "WebSuperAdmin");
        if (!privileged) return Unauthorized("You are not authorised to perform this action");

        var adminIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? User.Identity?.Name ?? "ERROR!";

        var result = await _penaltyService.RemovePenalty(request, adminIdentity);
        return result switch
        {
            true => Ok("Penalty deleted!"),
            false => BadRequest("Error removing penalty")
        };
    }
}
