using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Penalty.Commands;

public class GetPenaltiesPaginationCommand : Pagination, IRequest<PaginationContext<DTOs.WebView.PenaltiesView.Penalty>>
{
    public bool Privileged { get; set; }
}
