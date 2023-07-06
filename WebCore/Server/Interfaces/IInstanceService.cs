using Data.Enums;
using Data.Domains;

namespace BanHub.WebCore.Server.Interfaces;

public interface IInstanceService
{
    Task<(ControllerEnums.ReturnState, string)> CreateOrUpdateAsync(Instance request, string? requestIpAddress);
    Task<(ControllerEnums.ReturnState, Instance?)> GetInstanceAsync(string guid);
    Task<List<Instance>> PaginationAsync(Pagination pagination);
    Task<ControllerEnums.ReturnState> IsInstanceActiveAsync(string instanceGuid);
}
