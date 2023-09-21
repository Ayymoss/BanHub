using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Search;
using BanHub.WebCore.Shared.Models.SearchView;
using BanHubData.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Search>>> Search([FromQuery] string query)
    {
        if (query.Length < 3) return BadRequest();
        var result = await sender.Send(new GetSearchCommand {Query = query});
        if (result.State is ControllerEnums.ReturnState.BadRequest) return BadRequest();
        return Ok(result.Search);
    }
}
