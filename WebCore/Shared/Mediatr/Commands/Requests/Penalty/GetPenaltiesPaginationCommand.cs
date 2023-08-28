using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Penalty;

public class GetPenaltiesPaginationCommand : Pagination, IRequest<PaginationContext<Shared.Models.PenaltiesView.Penalty>>
{
    public bool Privileged { get; set; }
}
