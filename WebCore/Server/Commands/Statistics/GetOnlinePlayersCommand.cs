using MediatR;

namespace BanHub.WebCore.Server.Commands.Statistics;

public class GetOnlinePlayersCommand : IRequest<int>
{
}
