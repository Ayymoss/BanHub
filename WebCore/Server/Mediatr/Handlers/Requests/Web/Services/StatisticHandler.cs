using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;
using BanHub.WebCore.Server.Mediatr.Commands.Requests.Statistics;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Servers;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Services;

public class StatisticHandler : IRequestHandler<GetStatisticsCommand, Statistic>, IRequestHandler<GetOnlinePlayersCommand, int>,
    IRequestHandler<GetRecentBansCommand, int>, IRequestHandler<GetServerOnlineCountsCommand, Dictionary<string, int>>
{
    private readonly IStatisticsCache _statisticsCache;
    private readonly IMediator _mediator;

    public StatisticHandler(IStatisticsCache statisticsCache, IMediator mediator)
    {
        _statisticsCache = statisticsCache;
        _mediator = mediator;
    }

    public async Task<Statistic> Handle(GetStatisticsCommand request, CancellationToken cancellationToken)
    {
        if (!_statisticsCache.Loaded) await _mediator.Publish(new EnsureInitialisedNotification(), cancellationToken);

        return new Statistic
        {
            PenaltyCount = _statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.PenaltyCount),
            ServerCount = _statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.ServerCount),
            CommunityCount = _statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.CommunityCount),
            PlayerCount = _statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.PlayerCount),
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

    public Task<Dictionary<string, int>> Handle(GetServerOnlineCountsCommand request, CancellationToken cancellationToken)
    {
        var serverOnlineCount = new Dictionary<string, int>(/*_statisticsCache.ServerOnlineCount*/); // TODO: FIX
        return Task.FromResult(serverOnlineCount);
    }
}
