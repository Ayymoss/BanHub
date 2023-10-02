using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetProfilePenaltiesPaginationCommand : Pagination, IRequest<PaginationContext<DTOs.WebView.PlayerProfileView.Penalty>>
{
    public bool Privileged { get; set; }
    public string Identity { get; set; }
}
