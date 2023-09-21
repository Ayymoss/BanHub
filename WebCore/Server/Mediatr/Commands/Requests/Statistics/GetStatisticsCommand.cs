using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Commands.Requests.Statistics;

public class GetStatisticsCommand : IRequest<Statistic>
{
}
