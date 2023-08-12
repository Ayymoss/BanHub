using BanHub.WebCore.Server.Context;
using BanHubData.Commands.Player;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Player;

public class HasIdentityBanHandler : IRequestHandler<HasIdentityBanCommand, bool>
{
    private readonly DataContext _context;

    public HasIdentityBanHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(HasIdentityBanCommand request, CancellationToken cancellationToken)
    {
        var hasIdentityBan = await _context.PenaltyIdentifiers
            .Where(x => x.Expiration > DateTimeOffset.UtcNow)
            .Where(x => x.Identity == request.Identity)
            .AnyAsync(cancellationToken: cancellationToken);
        return hasIdentityBan;
    }
}
