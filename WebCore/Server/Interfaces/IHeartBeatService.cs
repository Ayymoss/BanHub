using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.Models;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IHeartBeatService
{
    Task<(ControllerEnums.ProfileReturnState, bool)> InstanceHeartbeat(InstanceDto request);
    Task EntitiesHeartbeat(List<EntityDto> request);
}
