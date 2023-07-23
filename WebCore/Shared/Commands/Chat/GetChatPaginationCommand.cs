using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.Chat;

public class GetChatPaginationCommand : Pagination, IRequest<ChatContext>
{
    public string PlayerIdentity { get; set; }
}
