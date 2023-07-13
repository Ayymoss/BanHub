using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Commands.Instance;
using BanHubData.Domains;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstanceController : ControllerBase
{
    private readonly IInstanceService _instanceService;
    private readonly IMediator _mediator;

    public InstanceController(IInstanceService instanceService, IMediator mediator)
    {
        _instanceService = instanceService;
        _mediator = mediator;
    }

    /// <summary>
    /// Creates or Updates an instance.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateOrUpdate([FromBody] CreateOrUpdateInstanceCommand request)
    {
        
        // HttpContext.Request.Headers["X-Forwarded-For"]
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
    public async Task<ActionResult<bool>> IsInstanceActive([FromBody] IsInstanceActiveCommand guid)
    {
        var result = await _mediator.Send(guid);
        if (!result) return Unauthorized();
        return Accepted();
    }

    [HttpGet]
    public async Task<ActionResult<Instance>> GetInstance([FromQuery] string guid)
    {
        var result = await _instanceService.GetInstanceAsync(guid);
        return result.Item1 switch
        {
            ControllerEnums.ReturnState.NotFound => NotFound("Instance not found"),
            ControllerEnums.ReturnState.Ok => Ok(result.Item2),
            _ => BadRequest() // Should never happen
        };
    }

    [HttpPost("All")]
    public async Task<ActionResult<IEnumerable<Instance>>> GetInstances([FromBody] Pagination pagination)
    {
        return Ok(await _instanceService.PaginationAsync(pagination));
    }
}
