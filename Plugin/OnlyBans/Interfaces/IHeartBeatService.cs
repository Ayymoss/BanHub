using Data.Commands.Heartbeat;
using Data.Domains;
using RestEase;

namespace BanHub.Interfaces;

public interface IHeartBeatService
{
    [Post("/HeartBeat/Instance")]
    Task<HttpResponseMessage> PostInstanceHeartBeat([Body] InstanceHeartbeatCommand instance);

    [Post("/HeartBeat/Players")]
    Task<HttpResponseMessage> PostPlayersHeartBeat([Body] PlayersHeartbeartCommand players, [Query("authToken")] string authToken);
}
