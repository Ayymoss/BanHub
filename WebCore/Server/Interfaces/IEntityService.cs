using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.Models;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IEntityService
{
    Task<EntityDto?> GetUser(string identity);
    Task<List<EntityDto>> GetUsers();
    Task<ControllerEnums.ProfileReturnState> CreateOrUpdate(EntityDto request);
    Task<bool> HasEntity(string identity);
    Task<int> GetEntityCount();
}
