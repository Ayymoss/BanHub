using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstanceController : Controller
{
    private readonly IInstanceService _instanceService;


    public InstanceController(IInstanceService instanceService)
    {
        _instanceService = instanceService;
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreateOrUpdate([FromBody] InstanceDto request)
    {
        var requestIpAddress = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        var result = await _instanceService.CreateOrUpdate(request, requestIpAddress);

        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.Created => StatusCode(201, result.Item2),
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest(result.Item2),
            ControllerEnums.ProfileReturnState.Conflict => StatusCode(409, result.Item2),
            ControllerEnums.ProfileReturnState.Accepted => StatusCode(202, result.Item2),
            ControllerEnums.ProfileReturnState.Ok => Ok(result.Item2),
            _ => BadRequest() // Should never happen
        };
    }

    [HttpGet]
    public async Task<ActionResult<InstanceDto>> GetServer([FromQuery] string guid)
    {
        var result = await _instanceService.GetServer(guid);
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.NotFound => NotFound("Instance not found"),
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest("Invalid guid"),
            ControllerEnums.ProfileReturnState.Ok => Ok(result.Item2),
            _ => BadRequest() // Should never happen
        };
    }
}
