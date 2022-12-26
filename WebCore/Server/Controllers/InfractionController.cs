using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Services;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InfractionController : Controller
{
    private readonly IInfractionService _infractionService;

    public InfractionController(IInfractionService infractionService)
    {
        _infractionService = infractionService;
    }

    [HttpPost, PluginAuthentication]
    public async Task<ActionResult<string>> AddInfraction([FromQuery] string authToken, [FromBody] InfractionDto request)
    {
        var result = await _infractionService.AddInfraction(request);
        return result.Item1 switch
        {
            ControllerEnums.ProfileReturnState.Created => Ok(result.Item2.HasValue ? result.Item2.Value : "Error"),
            ControllerEnums.ProfileReturnState.NotFound => NotFound(),
            ControllerEnums.ProfileReturnState.BadRequest => BadRequest(),
            ControllerEnums.ProfileReturnState.NotModified => StatusCode(304, "Infraction already exists"),
            _ => BadRequest() // Should never happen
        };
    }
}
