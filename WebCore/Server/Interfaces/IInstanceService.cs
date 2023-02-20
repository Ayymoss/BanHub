using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IInstanceService
{
    Task<(ControllerEnums.ReturnState, string)> CreateOrUpdateAsync(InstanceDto request, string? requestIpAddress);
    Task<(ControllerEnums.ReturnState, InstanceDto?)> GetInstanceAsync(string guid);
    Task<List<InstanceDto>> PaginationAsync(PaginationDto pagination);
    Task<ControllerEnums.ReturnState> IsInstanceActiveAsync(string instanceGuid);
}
