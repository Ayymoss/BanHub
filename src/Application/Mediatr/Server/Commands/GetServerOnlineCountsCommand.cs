using MediatR;

namespace BanHub.Application.Mediatr.Server.Commands;

public class GetServerOnlineCountsCommand : IRequest<Dictionary<string, int>>
{
    public string[] ServerIds { get; set; }
}
