using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IServerService
{
    Task<ControllerEnums.ReturnState> AddAsync(ServerDto request);
    Task<(ControllerEnums.ReturnState, ServerDto?)> GetAsync(string serverId);
}
