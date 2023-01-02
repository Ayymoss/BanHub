using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.Models;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IServerService
{
    Task<ControllerEnums.ProfileReturnState> Add(ServerDto request);
    Task<(ControllerEnums.ProfileReturnState, ServerDto?)> Get(string serverId);
}
