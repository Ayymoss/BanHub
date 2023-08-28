using BanHub.WebCore.Server.Mediatr.Commands.Events.Statistics;
using BanHub.WebCore.Server.Mediatr.Commands.Requests.Statistics;
using BanHub.WebCore.Server.Services;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Statistics;

public class StatisticHandler : IRequestHandler<GetStatisticsCommand, Statistic>, IRequestHandler<GetOnlinePlayersCommand, int>,
    IRequestHandler<GetRecentBansCommand, int>
{
    private readonly StatisticsCache _statisticsCache;
    private readonly IMediator _mediator;

    public StatisticHandler(StatisticsCache statisticsCache, IMediator mediator)
    {
        _statisticsCache = statisticsCache;
        _mediator = mediator;
    }

    public async Task<Statistic> Handle(GetStatisticsCommand request, CancellationToken cancellationToken)
    {
        if (!_statisticsCache.Loaded) await _mediator.Publish(new EnsureInitialisedNotification(), cancellationToken);

        return new Statistic
        {
            PenaltyCount = _statisticsCache.Penalties,
            ServerCount = _statisticsCache.Servers,
            CommunityCount = _statisticsCache.Communities,
            PlayerCount = _statisticsCache.Players,
        };
    }

    public Task<int> Handle(GetOnlinePlayersCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_statisticsCache.OnlinePlayers.Count);
    }

    public Task<int> Handle(GetRecentBansCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_statisticsCache.RecentBans.Count);
    }
}
