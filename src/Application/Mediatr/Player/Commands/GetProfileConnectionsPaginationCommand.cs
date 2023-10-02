using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetProfileConnectionsPaginationCommand : Pagination, IRequest<PaginationContext<Connection>>
{
    public string Identity { get; set; }
}
