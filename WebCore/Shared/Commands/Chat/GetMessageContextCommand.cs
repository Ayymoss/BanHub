using BanHub.WebCore.Shared.Models.PlayerProfileView;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.Chat;

public class GetMessageContextCommand : IRequest<ChatContextRoot>
{
    public DateTimeOffset Submitted { get; set; }
    public string ServerId { get; set; }
}
