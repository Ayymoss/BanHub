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
        // TODO This should expire any old bans before returning...
        var result = await _context.Players.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Identity == request.Identity, cancellationToken: cancellationToken);
        if (result is null) return false;

        return result.Penalties.Any(x => x is
        {
            PenaltyStatus: PenaltyStatus.Active,
            PenaltyType: PenaltyType.Ban,
            PenaltyScope: PenaltyScope.Global
        });
        
        //if (entity is null) return null;
//
        //// Check if user is globally banned
        //var hasActiveIdentityBan = await _context.PenaltyIdentifiers
        //    .Where(x => x.Expiration > DateTimeOffset.UtcNow)
        //    .AnyAsync(x => x.IpAddress == entity.IpAddress || x.Identity == entity.Identity
        //                   , cancellationToken: cancellationToken);
        //entity.HasIdentityBan = hasActiveIdentityBan;
//
        //// Check and expire infractions
        //var updatedPenalty = entity.Penalties
        //    .Where(inf => inf is {Duration: not null, PenaltyStatus: PenaltyStatus.Active} &&
        //                  DateTimeOffset.UtcNow > inf.Submitted + inf.Duration)
        //    .Select(inf =>
        //    {
        //        inf.PenaltyStatus = PenaltyStatus.Expired;
        //        return inf.PenaltyGuid;
        //    }).ToList();
//
        //if (!updatedPenalty.Any()) return entity;
//
        //var penalties = await _context.Penalties
        //    .AsTracking()
        //    .Where(x => updatedPenalty.Contains(x.PenaltyGuid))
        //    .ToListAsync();
//
        //foreach (var penalty in penalties)
        //{
        //    penalty.PenaltyStatus = PenaltyStatus.Expired;
        //    _context.Penalties.Update(penalty);
        //}
//
        //await _context.SaveChangesAsync();
//
        //return entity;
    }
}
