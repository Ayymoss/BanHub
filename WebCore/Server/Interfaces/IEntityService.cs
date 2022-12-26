using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.Models;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IEntityService
{
    public Task<EntityDto?> GetUser(string identity);
    public Task<List<EntityDto>> GetUsers();
    public Task<ControllerEnums.ProfileReturnState> CreateOrUpdate(EntityDto request);
}
