using GlobalInfraction.WebCore.Shared.Models;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IStatisticService
{
    Task<StatisticDto> GetStatistics();
}
