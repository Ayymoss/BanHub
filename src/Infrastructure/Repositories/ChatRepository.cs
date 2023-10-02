using BanHub.Application.DTOs.WebView.PlayerProfileView;
using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.ValueObjects.Repository.Chat;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class ChatRepository(DataContext context) : IChatRepository
{
    public async Task AddChatsAsync(IEnumerable<EFChat> chats, CancellationToken cancellationToken)
    {
        context.Chats.AddRange(chats);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<EFChat>> GetNewPlayerChatsAsync(string identity, DateTimeOffset lastChatCalculated,
        CancellationToken cancellationToken)
    {
        var newPlayerChats = await context.Chats
            .Where(x => x.Player.Identity == identity)
            .Where(x => x.Submitted > lastChatCalculated)
            .ToListAsync(cancellationToken: cancellationToken);
        return newPlayerChats;
    }

    public async Task<IEnumerable<SearchChat>> GetSearchChatsAsync(string query, CancellationToken cancellationToken)
    {
        var chats = await context.Chats
            .Where(search => EF.Functions.ILike(search.Message, $"%{query}%"))
            .Select(x => new SearchChat
            {
                Message = x.Message,
                Player = new SearchPlayer
                {
                    Identity = x.Player.Identity,
                    Username = x.Player.CurrentAlias.Alias.UserName
                }
            }).ToListAsync(cancellationToken: cancellationToken);

        return chats;
    }

    public async Task<int> GetPlayerChatCountAsync(string identity, CancellationToken cancellationToken)
    {
        var count = await context.Chats
            .Where(x => x.Player.Identity == identity)
            .CountAsync(cancellationToken: cancellationToken);
        return count;
    }

    public async Task<IEnumerable<ChatContext>> GetPlayerChatContextAsync(string serverId, DateTimeOffset submitted,
        CancellationToken cancellationToken)
    {
        var chats = await context.Chats
            .Where(x => x.Server.ServerId == serverId)
            .Where(x => x.Submitted >= submitted.AddMinutes(-5))
            .Where(x => x.Submitted < submitted.AddMinutes(5))
            .Select(x => new ChatContext
            {
                Submitted = x.Submitted,
                Message = x.Message,
                PlayerUserName = x.Player.CurrentAlias.Alias.UserName,
                PlayerIdentity = x.Player.Identity
            }).ToListAsync(cancellationToken: cancellationToken);

        return chats;
    }
}
