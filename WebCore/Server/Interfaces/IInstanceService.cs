using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IInstanceService
{
    Task<(ControllerEnums.ProfileReturnState, string)> CreateOrUpdate(InstanceDto request, string? requestIpAddress);
    Task<(ControllerEnums.ProfileReturnState, InstanceDto?)> GetInstance(string guid);
    Task<(ControllerEnums.ProfileReturnState, List<InstanceDto>?)> GetInstances();
    Task<ControllerEnums.ProfileReturnState> IsInstanceActive(string instanceGuid);
    Task<int> GetInstanceCount();
}
