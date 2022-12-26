using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.Models;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IHeartBeatService
{
    public Task<(ControllerEnums.ProfileReturnState, bool)> InstanceHeartbeat(InstanceDto request);
    public Task EntitiesHeartbeat(List<EntityDto> request);
}
