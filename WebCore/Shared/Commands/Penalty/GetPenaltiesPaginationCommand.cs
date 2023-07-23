using BanHub.WebCore.Shared.Models.PenaltiesView;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.Penalty;

public class GetPenaltiesPaginationCommand : Pagination, IRequest<PenaltyContext>
{
    public bool Privileged { get; set; }
}
