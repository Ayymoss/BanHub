using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Chat;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Chat;

public class GetChatCountHandler(DataContext context)
    : IRequestHandler<GetChatCountCommand, ChatCount>
{
    public async Task<ChatCount> Handle(GetChatCountCommand request, CancellationToken cancellationToken)
    {
        var count = await context.Chats
            .Where(x => x.Player.Identity == request.PlayerIdentity)
            .CountAsync(cancellationToken: cancellationToken);
        return new ChatCount {Count = count};
    }
}
