using BanHub.Domain.Entities;

namespace BanHub.Domain.Interfaces.Repositories;

public interface IPenaltyIdentifierRepository
{
    Task<bool> HasIdentityBanAsync(string identity, CancellationToken cancellationToken);
    Task AddPenaltyIdentifierAsync(EFPenaltyIdentifier penaltyIdentifier, CancellationToken cancellationToken);
    Task RemovePenaltyIdentifiersAsync(EFPenaltyIdentifier[] removeIdentifiers, CancellationToken cancellationToken);

    Task<IEnumerable<EFPenaltyIdentifier>> GetActivePenaltyIdentifiersAsync(string requestIpAddress, string requestIdentity,
        CancellationToken cancellationToken);
}
