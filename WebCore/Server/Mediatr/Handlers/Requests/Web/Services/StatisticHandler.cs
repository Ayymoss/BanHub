using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Statistics;
using BanHub.WebCore.Server.Mediatr.Commands.Requests.Statistics;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Servers;
using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Enums;
using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Services;

public class StatisticHandler(IStatisticsCache statisticsCache, IPublisher publisher) : IRequestHandler<GetStatisticsCommand, Statistic>,
    IRequestHandler<GetOnlinePlayersCommand, int>,
    IRequestHandler<GetRecentBansCommand, int>, IRequestHandler<GetServerOnlineCountsCommand, Dictionary<string, int>>
{
    public async Task<Statistic> Handle(GetStatisticsCommand request, CancellationToken cancellationToken)
    {
        if (!statisticsCache.Loaded) await publisher.Publish(new EnsureInitialisedNotification(), cancellationToken);

        return new Statistic
        {
            PenaltyCount = statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.PenaltyCount),
            ServerCount = statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.ServerCount),
            CommunityCount = statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.CommunityCount),
            PlayerCount = statisticsCache.GetStatisticCount(ControllerEnums.StatisticType.PlayerCount),
        };
    }

    public Task<int> Handle(GetOnlinePlayersCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(statisticsCache.OnlinePlayers.Count);
    }

    public Task<int> Handle(GetRecentBansCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(statisticsCache.RecentBans.Count);
    }

    public Task<Dictionary<string, int>> Handle(GetServerOnlineCountsCommand request, CancellationToken cancellationToken)
    {
        var serverOnlineCount = new Dictionary<string, int>(/*_statisticsCache.ServerOnlineCount*/); // TODO: FIX
        return Task.FromResult(serverOnlineCount);
    }
}
