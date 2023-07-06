using MediatR;

namespace Data.Commands.Heartbeat;

public class PlayersHeartbeartCommand : IRequest
{
    public Guid InstanceGuid { get; set; }
    public List<string> PlayerIdentities { get; set; }
}
