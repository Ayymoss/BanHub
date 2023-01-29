using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IEntityService
{
    Task<EntityDto?> GetUser(string identity);
    Task<ControllerEnums.ProfileReturnState> CreateOrUpdate(EntityDto request);
    Task<bool> HasEntity(string identity);
    Task<int> GetOnlineCount();
    Task<List<EntityDto>> Pagination(PaginationDto pagination);
    Task<string?> GetAuthenticationToken(EntityDto request);
}
