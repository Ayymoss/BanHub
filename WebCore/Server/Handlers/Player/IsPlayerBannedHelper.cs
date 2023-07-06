using BanHub.WebCore.Server.Context;
using Data.Commands.Player;
using Data.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Player;

public class IsPlayerBannedHelper : IRequestHandler<IsPlayerBannedCommand, bool>
{
    private readonly DataContext _context;

    public IsPlayerBannedHelper(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(IsPlayerBannedCommand request, CancellationToken cancellationToken)
    {
        var result = await _context.Players.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Identity == request.Identity, cancellationToken: cancellationToken);
        if (result is null) return false;

        return result.Penalties.Any(x => x is
        {
            PenaltyStatus: PenaltyStatus.Active,
            PenaltyType: PenaltyType.Ban,
            PenaltyScope: PenaltyScope.Global
        });
    }
}
