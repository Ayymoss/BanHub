using System.Collections.Concurrent;
using BanHub.Domain.Enums;
using BanHub.Domain.ValueObjects.Services;

namespace BanHub.Domain.Interfaces.Services;

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
