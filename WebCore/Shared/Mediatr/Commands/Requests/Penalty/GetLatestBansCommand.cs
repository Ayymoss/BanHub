using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Penalty;

public class GetLatestBansCommand : IRequest<IEnumerable<Models.IndexView.Penalty>>
{
    public bool Privileged { get; set; }
}
