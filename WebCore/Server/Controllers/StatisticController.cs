using BanHub.WebCore.Server.Mediatr.Commands.Requests.Statistics;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BanHub.WebCore.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatisticController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<Statistic>> Statistics()
    {
        var result = await _mediator.Send(new GetStatisticsCommand());
        return Ok(result);
    }
}
