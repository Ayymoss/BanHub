using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.PlayerProfile;

public class GetProfilePenaltiesPaginationCommand : Pagination, IRequest<PaginationContext<Shared.Models.PlayerProfileView.Penalty>>
{
    public bool Privileged { get; set; }
    public string Identity { get; set; }
}
