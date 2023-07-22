using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.Chat;

public class GetChatPaginationCommand : Pagination, IRequest<IEnumerable<Models.PlayerProfileView.Chat>>
{
    public string PlayerIdentity { get; set; }
}
