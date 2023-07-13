using MediatR;

namespace BanHub.WebCore.Shared.Commands.Penalty;

public class GetLatestBansCommand : IRequest<IEnumerable<Models.IndexView.Penalty>>
{
    
}
