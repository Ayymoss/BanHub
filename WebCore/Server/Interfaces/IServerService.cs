using Data.Enums;

namespace BanHub.WebCore.Server.Interfaces;

public interface IServerService
{
    Task<ControllerEnums.ReturnState> AddAsync(Data.Domains.Server request);
    Task<(ControllerEnums.ReturnState, Data.Domains.Server?)> GetAsync(string serverId);
}
