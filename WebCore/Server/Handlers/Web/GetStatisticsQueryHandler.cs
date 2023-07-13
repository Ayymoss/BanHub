using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Commands;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Server.Handlers.Web;

public class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQueryCommand, Statistic>
{
    private readonly IStatisticService _statisticService;

    public GetStatisticsQueryHandler(IStatisticService statisticService)
    {
        _statisticService = statisticService;
    }
    public Task<Statistic> Handle(GetStatisticsQueryCommand request, CancellationToken cancellationToken)
    {
        return _statisticService.GetStatisticsAsync();
    }
}
