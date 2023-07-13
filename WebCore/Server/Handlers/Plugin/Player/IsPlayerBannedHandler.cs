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
