using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Server.Models;
using GlobalInfraction.WebCore.Shared.DTOs;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IStatisticService
{
    Task UpdateStatistic(ControllerEnums.StatisticType statistic);
    Task<StatisticDto> GetStatistics();
}
