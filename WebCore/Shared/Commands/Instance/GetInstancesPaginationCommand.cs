using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.Instance;

public class GetInstancesPaginationCommand : Pagination, IRequest<IEnumerable<Models.InstancesView.Instance>>
{
    
}
