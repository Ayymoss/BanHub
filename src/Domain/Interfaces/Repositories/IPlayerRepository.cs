using BanHub.Domain.Entities;
using BanHub.Domain.ValueObjects.Repository.Chat;
using BanHub.Domain.ValueObjects.Repository.Note;
using BanHub.Domain.ValueObjects.Repository.Player;
using BanHub.Domain.ValueObjects.Services;

namespace BanHub.Domain.Interfaces.Repositories;

public interface IPlayerRepository
{
    Task<PlayerInfo?> GetPlayerIdAsync(string identity, CancellationToken cancellationToken);
    Task<PlayerProfile?> GetPlayerProfileViewAsync(string identity, CancellationToken cancellationToken);
    Task<IEnumerable<PlayerIdentityInfo>> GetPlayerIdentitiesAsync(IEnumerable<string> identities, CancellationToken cancellationToken);
    Task<IEnumerable<SearchPlayer>> GetPlayerIdentitiesAsync(string query, CancellationToken cancellationToken);
    Task<bool> IsGloballyBannedAsync(string identity, CancellationToken cancellationToken);
    Task<int> GetPlayerCountAsync(CancellationToken cancellationToken);
    Task<EFPlayer?> GetPlayerByIdentityAsync(string identity, CancellationToken cancellationToken);
    Task<WebUser?> GetWebUserAsync(int playerId, CancellationToken cancellationToken);
    Task<int> AddOrUpdatePlayerAsync(EFPlayer player, CancellationToken cancellationToken);
    Task UpdatePlayerRangeAsync(IEnumerable<EFPlayer> players, CancellationToken cancellationToken);
    Task<IEnumerable<EFPlayer>> GetPlayerRangeAsync(IEnumerable<string> identities, CancellationToken cancellationToken);
    Task<EFPlayer?> GetPlayerWithPenaltiesByIdentityAsync(string identity, CancellationToken cancellationToken);
}
