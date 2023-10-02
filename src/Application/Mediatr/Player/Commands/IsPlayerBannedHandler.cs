using BanHub.Domain.Entities;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class IsPlayerBannedHandler(IPenaltyIdentifierRepository penaltyIdentifierRepository, IPlayerRepository playerRepository,
        IPenaltyRepository penaltyRepository)
    : IRequestHandler<IsPlayerBannedCommand, bool>
{
    public async Task<bool> Handle(IsPlayerBannedCommand request, CancellationToken cancellationToken)
    {
        var identifiers = await penaltyIdentifierRepository
            .GetActivePenaltyIdentifiersAsync(request.IpAddress, request.Identity, cancellationToken);
        var efPenaltyIdentifiers = identifiers as EFPenaltyIdentifier[] ?? identifiers.ToArray();
        var hasIdentifierIpAddressBan = efPenaltyIdentifiers.Any(x => x.IpAddress == request.IpAddress);
        var hasIdentifierComboBan = efPenaltyIdentifiers.Any(x => x.Identity == request.Identity && x.IpAddress == request.IpAddress);

        // Cascade GUID on constant IP. This is to prevent players from evading bans by changing their GUID.
        if (hasIdentifierIpAddressBan && !hasIdentifierComboBan)
            await CreateComboBan(request.Identity, request.IpAddress,
                efPenaltyIdentifiers.First(x => x.IpAddress == request.IpAddress).PenaltyId, cancellationToken);

        var player = await playerRepository.GetPlayerWithPenaltiesByIdentityAsync(request.Identity, cancellationToken);
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

        await penaltyIdentifierRepository.AddPenaltyIdentifierAsync(newIdentifierBan, cancellationToken);
    }

    private async Task ExpireOldPenalties(EFPlayer player, CancellationToken cancellationToken)
    {
        var expiredPenalties = player.Penalties
            .Where(x => DateTimeOffset.UtcNow > x.Expiration)
            .Where(x => x.PenaltyStatus == PenaltyStatus.Active)
            .ToList();

        if (expiredPenalties.Count is 0) return;

        expiredPenalties.ForEach(x => x.PenaltyStatus = PenaltyStatus.Expired);
        await penaltyRepository.UpdatePenaltiesAsync(expiredPenalties, cancellationToken);
    }
}
