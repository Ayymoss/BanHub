using BanHub.WebCore.Shared.Commands;
using BanHub.WebCore.Shared.ViewModels;
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
        var result = await _mediator.Send(new GetStatisticsQueryCommand());
        return Ok(result);
    }
}
