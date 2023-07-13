using BanHub.WebCore.Shared.Models.Shared;
using BanHubData.Domains;
using BanHubData.Enums;

namespace BanHub.WebCore.Server.Interfaces;

public interface IInstanceService
{
    Task<(ControllerEnums.ReturnState, Instance?)> GetInstanceAsync(string guid);
    Task<List<Instance>> PaginationAsync(Pagination pagination);
}
