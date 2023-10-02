using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Chat.Commands;

public class GetProfileChatPaginationHandler(
        IResourceQueryHelper<GetProfileChatPaginationCommand, DTOs.WebView.PlayerProfileView.Chat> resourceQueryHelper)
    : IRequestHandler<GetProfileChatPaginationCommand, PaginationContext<DTOs.WebView.PlayerProfileView.Chat>>
{
    public async Task<PaginationContext<DTOs.WebView.PlayerProfileView.Chat>> Handle(GetProfileChatPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var result = await resourceQueryHelper.QueryResourceAsync(request, cancellationToken);
        return result;
    }
}
