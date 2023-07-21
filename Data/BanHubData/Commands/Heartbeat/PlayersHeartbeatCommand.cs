using MediatR;

namespace BanHubData.Commands.Heartbeat;

public class PlayersHeartbeatCommand : IRequest
{
    public Guid CommunityGuid { get; set; }
    public List<string> PlayerIdentities { get; set; }
}
