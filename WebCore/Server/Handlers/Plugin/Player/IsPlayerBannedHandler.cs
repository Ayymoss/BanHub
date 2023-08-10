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
            .Where(x => x.Identity == request.Identity)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var hasIpAddressBan = await _context.PenaltyIdentifiers
            .Where(x => x.Expiration > DateTimeOffset.UtcNow)
            .Where(x => x.IpAddress == request.IpAddress)
            .AnyAsync(cancellationToken: cancellationToken);

        if (hasIpAddressBan)
        {
            // They have an IP ban. Let's check if they correctly have a combo ban.
            var hasComboBan = await _context.PenaltyIdentifiers
                .Where(x => x.Expiration > DateTimeOffset.UtcNow)
                .Where(x => x.Identity == request.Identity)
                .Where(x => x.IpAddress == request.IpAddress)
                .AnyAsync(cancellationToken: cancellationToken);

            if (!hasComboBan)
            {
                // Doesn't have a combo ban, so let's create a new one. They're probably trying to evade a ban...
                var newIdentifierBan = new EFPenaltyIdentifier
                {
                    Identity = request.Identity,
                    IpAddress = request.IpAddress,
                    Expiration = DateTimeOffset.UtcNow.AddMonths(1)
                };

                _context.PenaltyIdentifiers.Add(newIdentifierBan);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        // Expire old penalties
        var playerIdentityBan = false;
        if (player is not null)
        {
            var expiredPenalties = player.Penalties
                .Where(x => DateTimeOffset.UtcNow > x.Expiration)
                .Where(x => x.PenaltyStatus == PenaltyStatus.Active)
                .ToList();

            if (expiredPenalties.Any())
            {
                expiredPenalties.ForEach(x => x.PenaltyStatus = PenaltyStatus.Expired);
                _context.Penalties.UpdateRange(expiredPenalties);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Check player identifier states
            playerIdentityBan = player.Penalties.Any(x => x is
            {
                PenaltyStatus: PenaltyStatus.Active,
                PenaltyType: PenaltyType.Ban,
                PenaltyScope: PenaltyScope.Global
            });
        }

        return playerIdentityBan || hasIpAddressBan;
    }
}
