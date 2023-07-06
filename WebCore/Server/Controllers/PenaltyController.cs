using System.Security.Claims;
using Data.Enums;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Utilities;
using Data.Commands;
using Data.Commands.Penalty;
using Data.Commands.Player;
using Data.Domains;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PenaltyController : ControllerBase
{
    private readonly IPenaltyService _penaltyService;
    private readonly IMediator _mediator;

    public PenaltyController(IPenaltyService penaltyService, IMediator mediator)
    {
        _penaltyService = penaltyService;
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

    [HttpGet]
    public async Task<ActionResult<Instance>> GetPenalty([FromQuery] string guid)
    {
        var result = await _penaltyService.GetPenaltiesAsync(guid);
        return result.Item1 switch
        {
            ControllerEnums.ReturnState.Ok => Ok(result.Item2?.FirstOrDefault()),
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.BadRequest => BadRequest(),
            _ => BadRequest() // Should never happen
        };
    }

    [HttpPost("All")]
    public async Task<ActionResult<IEnumerable<Penalty>>> GetPenalties([FromBody] Pagination pagination)
    {
        return Ok(await _penaltyService.PaginationAsync(pagination));
    }

    [HttpGet("Index")]
    public async Task<ActionResult<IEnumerable<Penalty>>> GetRecentPenalties()
    {
        return Ok(await _penaltyService.GetLatestThreeBansAsync());
    }

    [HttpPost("Remove")] // Authorised endpoint
    public async Task<ActionResult<bool>> RemovePenalty([FromBody] Penalty request)
    {
        var privileged = User.IsInAnyRole("WebAdmin", "WebSuperAdmin");
        var adminIdentity = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (!privileged || adminIdentity is null) return Unauthorized("You are not authorised to perform this action");
        
        var result = await _penaltyService.RemovePenaltyAsync(request, adminIdentity);
        return result switch
        {
            true => Ok("Penalty deleted!"),
            false => BadRequest("Error removing penalty")
        };
    }
}
