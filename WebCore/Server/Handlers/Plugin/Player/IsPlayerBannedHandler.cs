using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Models.Domains;
using BanHubData.Commands.Player;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Player;

public class IsPlayerBannedHandler : IRequestHandler<IsPlayerBannedCommand, bool>
{
    private readonly DataContext _context;

    public IsPlayerBannedHandler(DataContext context)
    {
        _context = context;
    }

    // TODO: Consider improving this. Right now users who change both identifiers will be unbanned.
    // Ban reference system? https://chat.openai.com/c/e8853a97-6810-4154-a08a-6812404a39f4
    public async Task<bool> Handle(IsPlayerBannedCommand request, CancellationToken cancellationToken)
    {
        var player = await _context.Players
            .AsNoTracking()
            .Include(x => x.Penalties)
            .FirstOrDefaultAsync(x => x.Identity == request.Identity, cancellationToken: cancellationToken);

        var hasIdentifierBan = await _context.PenaltyIdentifiers
            .Where(x => x.Expiration > DateTimeOffset.UtcNow)
            // The identity check below is redundant, however, may be relevant in the future
            .AnyAsync(x => x.IpAddress == request.IpAddress || x.Identity == request.Identity, cancellationToken: cancellationToken);

        // Expire old penalties
        var playerIdentityBan = false;
        if (player is not null)
        {
            var expiredPenalties = player.Penalties
                .Where(inf => DateTimeOffset.UtcNow > inf.Expiration)
                .Where(inf => inf.PenaltyStatus == PenaltyStatus.Active)
                .ToList();

            expiredPenalties.ForEach(inf => inf.PenaltyStatus = PenaltyStatus.Expired);
            _context.Penalties.UpdateRange(expiredPenalties);
            await _context.SaveChangesAsync(cancellationToken);

            // Check player identifier states
            playerIdentityBan = player.Penalties.Any(penalty => penalty is
            {
                PenaltyStatus: PenaltyStatus.Active,
                PenaltyType: PenaltyType.Ban,
                PenaltyScope: PenaltyScope.Global
            });
        }

        var isBanned = playerIdentityBan || hasIdentifierBan;
        return isBanned;
    }
}
