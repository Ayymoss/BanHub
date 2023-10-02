using BanHub.Domain.Entities;
using BanHub.Domain.Enums;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.ValueObjects.Repository.Chat;
using BanHub.Domain.ValueObjects.Repository.Player;
using BanHub.Domain.ValueObjects.Services;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class PlayerRepository(DataContext context) : IPlayerRepository
{
    public Task<PlayerInfo?> GetPlayerIdAsync(string identity, CancellationToken cancellationToken)
    {
        return context.Players
            .Where(x => x.Identity == identity)
            .Select(x => new PlayerInfo
            {
                Id = x.Id,
                Identity = x.Identity,
                UserName = x.CurrentAlias.Alias.UserName,
                IpAddress = x.CurrentAlias.Alias.IpAddress
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<PlayerProfile?> GetPlayerProfileViewAsync(string identity, CancellationToken cancellationToken)
    {
        var entity = await context.Players
            .Where(profile => profile.Identity == identity)
            .Select(profile => new PlayerProfile
            {
                Identity = profile.Identity,
                UserName = profile.CurrentAlias.Alias.UserName,
                Heartbeat = profile.Heartbeat,
                IpAddress = profile.CurrentAlias.Alias.IpAddress,
                Connected = profile.Heartbeat + TimeSpan.FromMinutes(5) > DateTimeOffset.UtcNow,
                TotalConnections = profile.TotalConnections,
                PlayTime = profile.PlayTime,
                Created = profile.Created,
                CommunityRole = profile.CommunityRole,
                WebRole = profile.WebRole,
                PenaltyCount = profile.Penalties.Count,
                NoteCount = profile.Notes.Count,
                ChatCount = profile.Chats.Count
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return entity;
    }

    public async Task<IEnumerable<PlayerIdentityInfo>> GetPlayerIdentitiesAsync(IEnumerable<string> identities,
        CancellationToken cancellationToken)
    {
        var players = await context.Players
            .Where(x => identities.Contains(x.Identity))
            .Select(x => new PlayerIdentityInfo
            {
                Id = x.Id,
                Identity = x.Identity
            }).ToListAsync(cancellationToken: cancellationToken);
        return players;
    }

    public async Task<IEnumerable<SearchPlayer>> GetPlayerIdentitiesAsync(string query, CancellationToken cancellationToken)
    {
        var players = await context.Players
            .Where(search =>
                EF.Functions.ILike(search.CurrentAlias.Alias.UserName, $"%{query}%") ||
                EF.Functions.ILike(search.Identity, $"%{query}%"))
            .Select(x => new SearchPlayer
            {
                Identity = x.Identity,
                Username = x.CurrentAlias.Alias.UserName
            }).ToListAsync(cancellationToken: cancellationToken);
        return players;
    }

    public async Task<bool> IsGloballyBannedAsync(string identity, CancellationToken cancellationToken)
    {
        var isGloballyBanned = await context.Players
            .Where(x => x.Identity == identity)
            .SelectMany(x => x.Penalties)
            .Where(x => x.PenaltyType == PenaltyType.Ban)
            .Where(x => x.PenaltyStatus == PenaltyStatus.Active)
            .AnyAsync(x => x.PenaltyScope == PenaltyScope.Global, cancellationToken: cancellationToken);

        return isGloballyBanned;
    }

    public async Task<int> GetPlayerCountAsync(CancellationToken cancellationToken)
    {
        return await context.Players.CountAsync(cancellationToken: cancellationToken);
    }

    public async Task<int> AddOrUpdatePlayerAsync(EFPlayer player, CancellationToken cancellationToken)
    {
        var efPlayer = await context.Players
            .Where(x => x.Id == player.Id)
            .AnyAsync(cancellationToken: cancellationToken);

        if (efPlayer) context.Players.Update(player);
        else context.Players.Add(player);

        await context.SaveChangesAsync(cancellationToken);
        return player.Id;
    }

    public async Task<IEnumerable<EFPlayer>> GetPlayerRangeAsync(IEnumerable<string> identities, CancellationToken cancellationToken)
    {
        var profiles = await context.Players
            .AsTracking()
            .Where(p => identities.Contains(p.Identity))
            .ToListAsync(cancellationToken: cancellationToken);
        return profiles;
    }

    public async Task<EFPlayer?> GetPlayerWithPenaltiesByIdentityAsync(string identity, CancellationToken cancellationToken)
    {
        var player = await context.Players
            .AsNoTracking()
            .Include(x => x.Penalties)
            .Where(x => x.Identity == identity)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return player;
    }

    public async Task UpdatePlayerRangeAsync(IEnumerable<EFPlayer> players, CancellationToken cancellationToken)
    {
        context.Players.UpdateRange(players);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<EFPlayer?> GetPlayerByIdentityAsync(string identity, CancellationToken cancellationToken)
    {
        var player = await context.Players
            .Where(x => x.Identity == identity)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return player;
    }

    public async Task<WebUser?> GetWebUserAsync(int playerId, CancellationToken cancellationToken)
    {
        var user = await context.Players
            .Where(x => x.Id == playerId)
            .Select(x => new WebUser
            {
                UserName = x.CurrentAlias.Alias.UserName,
                WebRole = x.WebRole.ToString(),
                CommunityRole = x.CommunityRole.ToString(),
                Identity = x.Identity,
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return user;
    }
}
