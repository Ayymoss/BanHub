using BanHub.WebCore.Shared.Models.PlayerProfileView;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Chat;

public class GetChatCountCommand : IRequest<ChatCount>
{
    public string PlayerIdentity { get; set; }
}
