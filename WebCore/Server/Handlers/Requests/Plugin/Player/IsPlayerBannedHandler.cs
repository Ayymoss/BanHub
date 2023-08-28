using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Models.Domains;
using BanHubData.Commands.Player;
using BanHubData.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Requests.Plugin.Player;

public class IsPlayerBannedHandler : IRequestHandler<IsPlayerBannedCommand, bool>
{
    private readonly DataContext _context;

    public IsPlayerBannedHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(IsPlayerBannedCommand request, CancellationToken cancellationToken)
    {
        var identifiers = await _context.PenaltyIdentifiers
            .Where(x => x.Expiration > DateTimeOffset.UtcNow)
            .Where(x => x.IpAddress == request.IpAddress || x.Identity == request.Identity)
            .ToListAsync(cancellationToken: cancellationToken);

        var hasIdentifierIpAddressBan = identifiers.Any(x => x.IpAddress == request.IpAddress);
        var hasIdentifierComboBan = identifiers
            .Where(x => x.Identity == request.Identity)
            .Any(x => x.IpAddress == request.IpAddress);

        // Cascade GUID on constant IP. This is to prevent players from evading bans by changing their GUID.
        if (hasIdentifierIpAddressBan && !hasIdentifierComboBan)
            await CreateComboBan(request.Identity, request.IpAddress, identifiers
                .First(x => x.IpAddress == request.IpAddress).PenaltyId, cancellationToken);

        // Get player context
        var player = await _context.Players
            .AsNoTracking()
            .Include(x => x.Penalties)
            .Where(x => x.Identity == request.Identity)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (player is not null) await ExpireOldPenalties(player, cancellationToken);

        var playerIdentityBan = player?.Penalties.Any(x => x is
        {
            PenaltyStatus: PenaltyStatus.Active,
            PenaltyType: PenaltyType.Ban,
            PenaltyScope: PenaltyScope.Global
        }) ?? false;

        return playerIdentityBan || hasIdentifierIpAddressBan;
    }

    private async Task CreateComboBan(string identity, string ipAddress, int penaltyId, CancellationToken cancellationToken)
    {
        var newIdentifierBan = new EFPenaltyIdentifier
        {
            Identity = identity,
            IpAddress = ipAddress,
            Expiration = DateTimeOffset.UtcNow.AddMonths(1),
            PenaltyId = penaltyId
        };

        _context.PenaltyIdentifiers.Add(newIdentifierBan);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task ExpireOldPenalties(EFPlayer player, CancellationToken cancellationToken)
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
    }
}
