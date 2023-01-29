using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IServerService
{
    Task<ControllerEnums.ProfileReturnState> Add(ServerDto request);
    Task<(ControllerEnums.ProfileReturnState, ServerDto?)> Get(string serverId);
}
