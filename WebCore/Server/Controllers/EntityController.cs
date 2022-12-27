using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Interfaces;
using GlobalInfraction.WebCore.Server.Services;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInfraction.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntityController : Controller
{
    private readonly IEntityService _entityService;

    public EntityController(IEntityService entityService)
    {
        _entityService = entityService;
    }

    [HttpPost, PluginAuthentication]
    public async Task<ActionResult> CreateOrUpdate([FromQuery] string authToken, [FromBody] EntityDto request)
    {
        return await _entityService.CreateOrUpdate(request) switch
        {
            ControllerEnums.ProfileReturnState.Updated => NoContent(),
            ControllerEnums.ProfileReturnState.Created => StatusCode(StatusCodes.Status201Created),
            _ => BadRequest() // Should be unreachable
        };
    }

    [HttpGet("All")]
    public async Task<ActionResult<IEnumerable<EntityDto>>> GetEntities()
    {
        return Ok(await _entityService.GetUsers());
    }

    [HttpGet]
    public async Task<ActionResult<EntityDto>> GetEntity([FromQuery] string identity)
    {
        var result = await _entityService.GetUser(identity);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("Exists")]
    public async Task<ActionResult<bool>> EntityExists([FromQuery] string identity)
    {
        var result = await _entityService.HasEntity(identity);
        if (!result) return NotFound(result);
        return Ok(result);
    }
}
