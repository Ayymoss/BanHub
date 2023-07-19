using BanHub.WebCore.Server.Context;
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

    public async Task<bool> Handle(IsPlayerBannedCommand request, CancellationToken cancellationToken)
    {
        var player = await _context.Players
            .Include(x => x.Penalties)
            .FirstOrDefaultAsync(x => x.Identity == request.Identity, cancellationToken: cancellationToken);

        var hasIpAddressBan = await _context.PenaltyIdentifiers
            .Where(x => x.Expiration > DateTimeOffset.UtcNow)
            .AnyAsync(x => x.IpAddress == request.IpAddress, cancellationToken: cancellationToken);

        if (player is null) return false;

        var expiredPenalties = player.Penalties
            .Where(inf => DateTimeOffset.UtcNow > inf.Expiration)
            .Where(inf => inf is {PenaltyStatus: PenaltyStatus.Active}).ToList();

        foreach (var penalty in expiredPenalties)
        {
            penalty.PenaltyStatus = PenaltyStatus.Expired;
            _context.Penalties.Update(penalty);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var hasIdentityBan = player.Penalties.Any(penalty => penalty is
        {
            PenaltyStatus: PenaltyStatus.Active,
            PenaltyType: PenaltyType.Ban,
            PenaltyScope: PenaltyScope.Global
        });

        return hasIdentityBan || hasIpAddressBan;
    }
}
