using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetProfileChatPaginationCommand : Pagination, IRequest<PaginationContext<DTOs.WebView.PlayerProfileView.Chat>>
{
    public string Identity { get; set; }
}
