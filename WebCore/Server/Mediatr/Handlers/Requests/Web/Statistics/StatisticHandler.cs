using BanHub.WebCore.Server.Mediatr.Commands.Events.Statistics;
using BanHub.WebCore.Server.Mediatr.Commands.Requests.Statistics;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Statistics;

public class StatisticHandler : IRequestHandler<GetStatisticsCommand, Statistic>, IRequestHandler<GetOnlinePlayersCommand, int>,
    IRequestHandler<GetRecentBansCommand, int>
{
    private readonly StatisticsTracking _statisticsTracking;
    private readonly IMediator _mediator;

    public StatisticHandler(StatisticsTracking statisticsTracking, IMediator mediator)
    {
        _statisticsTracking = statisticsTracking;
        _mediator = mediator;
    }

    public async Task<Statistic> Handle(GetStatisticsCommand request, CancellationToken cancellationToken)
    {
        if (!_statisticsTracking.Loaded) await _mediator.Publish(new EnsureInitialisedNotification(), cancellationToken);

        return new Statistic
        {
            PenaltyCount = _statisticsTracking.Penalties,
            ServerCount = _statisticsTracking.Servers,
            CommunityCount = _statisticsTracking.Communities,
            PlayerCount = _statisticsTracking.Players,
        };
    }

    public Task<int> Handle(GetOnlinePlayersCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_statisticsTracking.OnlinePlayers.Count);
    }

    public Task<int> Handle(GetRecentBansCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_statisticsTracking.RecentBans.Count);
    }
}
