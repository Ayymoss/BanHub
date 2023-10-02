using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetPlayersPaginationCommand : Pagination, IRequest<PaginationContext<DTOs.WebView.PlayersView.Player>>
{
}
