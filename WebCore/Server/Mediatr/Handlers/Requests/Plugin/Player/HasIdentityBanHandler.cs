using BanHub.WebCore.Server.Context;
using BanHubData.Mediatr.Commands.Requests.Player;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Plugin.Player;

public class HasIdentityBanHandler(DataContext context) : IRequestHandler<HasIdentityBanCommand, bool>
{
    public async Task<bool> Handle(HasIdentityBanCommand request, CancellationToken cancellationToken)
    {
        var hasIdentityBan = await context.PenaltyIdentifiers
            .Where(x => x.Expiration > DateTimeOffset.UtcNow)
            .Where(x => x.Identity == request.Identity)
            .AnyAsync(cancellationToken: cancellationToken);
        return hasIdentityBan;
    }
}
