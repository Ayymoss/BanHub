using BanHub.Application.DTOs.WebView.Shared;
using MediatR;

namespace BanHub.Application.Mediatr.Services.Commands.Statistics;

public class GetStatisticsCommand : IRequest<Statistic>
{
}
