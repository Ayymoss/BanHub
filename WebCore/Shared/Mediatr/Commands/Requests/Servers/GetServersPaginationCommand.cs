using BanHub.WebCore.Shared.Models.ServersView;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Servers;

public class GetServersPaginationCommand : Pagination, IRequest<PaginationContext<Server>>
{
}
