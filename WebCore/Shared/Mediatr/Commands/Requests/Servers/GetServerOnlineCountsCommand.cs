using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Servers;

public class GetServerOnlineCountsCommand : IRequest<Dictionary<string, int>>
{
    public string[] ServerIds { get; set; }
}
