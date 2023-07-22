using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.Chat;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Chat;

public class GetChatCountHandler : IRequestHandler<GetChatCountCommand, ChatCount>
{
    private readonly DataContext _context;

    public GetChatCountHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<ChatCount> Handle(GetChatCountCommand request, CancellationToken cancellationToken)
    {
        var count = await _context.Chats
            .Where(x => x.Player.Identity == request.PlayerIdentity)
            .CountAsync(cancellationToken: cancellationToken);
        return new ChatCount {Count = count};
    }
}
