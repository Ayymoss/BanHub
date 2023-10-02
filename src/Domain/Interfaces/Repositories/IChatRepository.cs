using BanHub.Domain.Entities;
using BanHub.Domain.ValueObjects.Repository.Chat;

namespace BanHub.Domain.Interfaces.Repositories;

public interface IChatRepository
{
    Task AddChatsAsync(IEnumerable<EFChat> chats, CancellationToken cancellationToken);

    Task<IEnumerable<EFChat>> GetNewPlayerChatsAsync(string identity, DateTimeOffset lastChatCalculated,
        CancellationToken cancellationToken);

    Task<IEnumerable<SearchChat>> GetSearchChatsAsync(string query, CancellationToken cancellationToken);
    Task<int> GetPlayerChatCountAsync(string identity, CancellationToken cancellationToken);

    Task<IEnumerable<ChatContext>>
        GetPlayerChatContextAsync(string serverId, DateTimeOffset submitted, CancellationToken cancellationToken);
}
