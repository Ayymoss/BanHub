using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Server.Commands;

public class GetServersPaginationCommand : Pagination, IRequest<PaginationContext<DTOs.WebView.ServersView.Server>>
{
}
