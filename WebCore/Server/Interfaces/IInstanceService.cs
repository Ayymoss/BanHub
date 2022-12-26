using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.Models;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IInstanceService
{
    public Task<(ControllerEnums.ProfileReturnState, string)> CreateOrUpdate(InstanceDto request, string? requestIpAddress);
    public Task<(ControllerEnums.ProfileReturnState,InstanceDto?)> GetServer(string guid);

}
