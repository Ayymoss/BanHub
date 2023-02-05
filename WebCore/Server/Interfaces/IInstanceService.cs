using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IInstanceService
{
    Task<(ControllerEnums.ProfileReturnState, string)> CreateOrUpdate(InstanceDto request, string? requestIpAddress);
    Task<(ControllerEnums.ProfileReturnState, InstanceDto?)> GetInstance(string guid);
    Task<List<InstanceDto>> Pagination(PaginationDto pagination);
    Task<ControllerEnums.ProfileReturnState> IsInstanceActive(string instanceGuid);
}
