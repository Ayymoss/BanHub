using Data.Commands;
using Data.Commands.Player;
using Data.Domains;

namespace BanHub.WebCore.Server.Interfaces;

public interface IPlayerService
{
    Task<bool> HasEntityAsync(string identity);
    Task<List<Player>> PaginationAsync(Pagination pagination);
    Task<string?> GetAuthenticationTokenAsync(string identity);
    Task<bool> AddNoteAsync(Note request, string adminIdentity);
    Task<bool> RemoveNoteAsync(Note request, string adminIdentity);
}
