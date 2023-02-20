using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IHeartBeatService
{
    Task<(ControllerEnums.ReturnState, bool)> InstanceHeartbeatAsync(InstanceDto request);
    Task EntitiesHeartbeatAsync(List<EntityDto> request);
}
