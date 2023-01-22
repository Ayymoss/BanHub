using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.DTOs;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IServerService
{
    Task<ControllerEnums.ProfileReturnState> Add(ServerDto request);
    Task<(ControllerEnums.ProfileReturnState, ServerDto?)> Get(string serverId);
}
