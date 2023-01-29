using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IHeartBeatService
{
    Task<(ControllerEnums.ProfileReturnState, bool)> InstanceHeartbeat(InstanceDto request);
    Task EntitiesHeartbeat(List<EntityDto> request);
}
