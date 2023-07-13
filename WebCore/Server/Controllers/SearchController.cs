using BanHub.WebCore.Shared.Commands.Search;
using BanHub.WebCore.Shared.Models.SearchView;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{query}")]
    public async Task<ActionResult<IEnumerable<Search>>> Search([FromQuery] string query)
    {
        if (query.Length < 3) return BadRequest();
        var result = await _mediator.Send(new GetSearchCommand {Query = query});
        return Ok(result);
    }
}
