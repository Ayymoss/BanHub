using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.PlayerProfile;

public class GetProfileChatPaginationCommand : Pagination, IRequest<PaginationContext<Models.PlayerProfileView.Chat>>
{
    public string Identity { get; set; }
}
