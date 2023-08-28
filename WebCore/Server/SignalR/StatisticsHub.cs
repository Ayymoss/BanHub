using BanHub.WebCore.Server.Mediatr.Commands.Requests.Statistics;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace BanHub.WebCore.Server.SignalR;

public class StatisticsHub : Hub
{
    private readonly IMediator _mediator;

    public StatisticsHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<int> GetCurrentOnlinePlayers() => await _mediator.Send(new GetOnlinePlayersCommand());

    public async Task<int> GetCurrentRecentBans() => await _mediator.Send(new GetRecentBansCommand());
}
