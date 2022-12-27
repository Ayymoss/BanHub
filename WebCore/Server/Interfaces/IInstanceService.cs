using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IInstanceService
{
    Task<(ControllerEnums.ProfileReturnState, string)> CreateOrUpdate(InstanceDto request, string? requestIpAddress);
    Task<(ControllerEnums.ProfileReturnState, InstanceDto?)> GetServer(string guid);
    Task<ControllerEnums.ProfileReturnState> IsInstanceActive(string instanceGuid);
}
