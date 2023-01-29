using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IStatisticService
{
    Task UpdateStatistic(ControllerEnums.StatisticType statistic);
    Task<StatisticDto> GetStatistics();
}
