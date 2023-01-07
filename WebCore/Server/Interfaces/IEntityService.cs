using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.Models;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IEntityService
{
    Task<EntityDto?> GetUser(string identity);
    Task<ControllerEnums.ProfileReturnState> CreateOrUpdate(EntityDto request);
    Task<bool> HasEntity(string identity);
    Task<int> GetOnlineCount();
    Task<List<EntityDto>> Pagination(PaginationDto pagination);
}
