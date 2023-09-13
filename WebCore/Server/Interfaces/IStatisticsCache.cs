using System.Collections.Concurrent;
using BanHub.WebCore.Server.Services;
using BanHubData.Enums;

namespace BanHub.WebCore.Server.Interfaces;

public interface IStatisticsCache
{
    ConcurrentDictionary<Guid, DateTimeOffset> RecentBans { get; }
    ConcurrentDictionary<string, DateTimeOffset> OnlinePlayers { get; }
    ConcurrentDictionary<string, ServerOnlineStatistic> OnlineServers { get; }
    bool Loaded { get; set; }

    void UpdateStatisticCount(ControllerEnums.StatisticType type, ControllerEnums.StatisticTypeAction action);
    int GetStatisticCount(ControllerEnums.StatisticType type);
    void SetStatisticsCount(ControllerEnums.StatisticType type, int count);
}
