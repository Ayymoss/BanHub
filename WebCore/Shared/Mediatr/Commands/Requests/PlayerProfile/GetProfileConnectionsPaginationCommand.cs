using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;

public class GetProfileConnectionsPaginationCommand : Pagination, IRequest<PaginationContext<Connection>>
{
    public string Identity { get; set; }
}
