using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IEntityService
{
    Task<EntityDto?> GetUserAsync(string identity, bool privileged);
    Task<ControllerEnums.ReturnState> CreateOrUpdateAsync(EntityDto request);
    Task<bool> HasEntityAsync(string identity);
    Task<List<EntityDto>> PaginationAsync(PaginationDto pagination);
    Task<string?> GetAuthenticationTokenAsync(EntityDto request);
    Task<bool> AddNoteAsync(NoteDto request, string adminIdentity);
    Task<bool> RemoveNoteAsync(NoteDto request, string adminIdentity);
}
