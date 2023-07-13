using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Domains;

namespace BanHub.WebCore.Server.Interfaces;

public interface IPlayerService
{
    Task<bool> HasEntityAsync(string identity);
    Task<List<Player>> PaginationAsync(Pagination pagination);
    Task<bool> AddNoteAsync(Note request, string adminIdentity);
    Task<bool> RemoveNoteAsync(Note request, string adminIdentity);
}
