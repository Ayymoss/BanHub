using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.Models;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IStatisticService
{
    Task UpdateStatistic(ControllerEnums.StatisticType statistic);
    Task<StatisticDto> GetStatistics();
}
