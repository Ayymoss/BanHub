using Data.Enums;
using BanHub.WebCore.Server.Interfaces;
using Data.Domains;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstanceController : ControllerBase
{
    private readonly IInstanceService _instanceService;


    public InstanceController(IInstanceService instanceService)
    {
        _instanceService = instanceService;
    }

    /// <summary>
    /// Creates or Updates an instance.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<string>> CreateOrUpdate([FromBody] Instance request)
    {
        var result = await _instanceService.CreateOrUpdateAsync(request, request.InstanceIp);
        return result.Item1 switch
        {
            ControllerEnums.ReturnState.Created => StatusCode(StatusCodes.Status201Created, result.Item2), // New, added
            ControllerEnums.ReturnState.BadRequest => BadRequest(result.Item2), // ??
            ControllerEnums.ReturnState.Conflict => StatusCode(StatusCodes.Status409Conflict, result.Item2), // Conflicting GUIDs
            ControllerEnums.ReturnState.Accepted => StatusCode(StatusCodes.Status202Accepted, result.Item2), // Activated
            ControllerEnums.ReturnState.Ok => Ok(result.Item2), // Not activated
            _ => BadRequest() // Should never happen
        };
    }

    [HttpGet("Active")]
    public async Task<ActionResult<bool>> IsInstanceActive([FromQuery] string guid)
    {
        var result = await _instanceService.IsInstanceActiveAsync(guid);
        return result switch
        {
            ControllerEnums.ReturnState.NotFound => NotFound(),
            ControllerEnums.ReturnState.BadRequest => BadRequest(),
            ControllerEnums.ReturnState.Accepted => Accepted(true), // Activated
            ControllerEnums.ReturnState.Unauthorized => Unauthorized(false),
            _ => BadRequest() // Should never happen
        };
    }

    [HttpGet]
    public async Task<ActionResult<Instance>> GetInstance([FromQuery] string guid)
    {
        var result = await _instanceService.GetInstanceAsync(guid);
        return result.Item1 switch
        {
            ControllerEnums.ReturnState.NotFound => NotFound("Instance not found"),
            ControllerEnums.ReturnState.BadRequest => BadRequest("Invalid guid"),
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
