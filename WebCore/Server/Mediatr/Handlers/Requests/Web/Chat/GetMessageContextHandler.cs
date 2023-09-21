using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Chat;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Chat;

public class GetMessageContextHandler(DataContext context) 
    : IRequestHandler<GetMessageContextCommand, ChatContextRoot>
{
    public async Task<ChatContextRoot> Handle(GetMessageContextCommand request, CancellationToken cancellationToken)
    {
        var chats = await context.Chats
            .Where(x => x.Server.ServerId == request.ServerId)
            .Where(x => x.Submitted >= request.Submitted.AddMinutes(-5))
            .Where(x => x.Submitted < request.Submitted.AddMinutes(5))
            .Select(x => new ChatContext
            {
                Submitted = x.Submitted,
                Message = x.Message,
                PlayerUserName = x.Player.CurrentAlias.Alias.UserName,
                PlayerIdentity = x.Player.Identity
            }).ToListAsync(cancellationToken: cancellationToken);

        var result = new ChatContextRoot
        {
            Messages = chats.OrderBy(x => x.Submitted),
            Loaded = true
        };
        return result;
    }
}
