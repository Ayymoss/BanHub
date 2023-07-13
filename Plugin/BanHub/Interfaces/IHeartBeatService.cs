using BanHubData.Commands.Heartbeat;
using RestEase;

namespace BanHub.Interfaces;

public interface IHeartBeatService
{
    [Post("/HeartBeat/Instance")]
    Task<HttpResponseMessage> PostInstanceHeartBeat([Body] InstanceHeartbeatCommand instance);

    [Post("/HeartBeat/Players")]
    Task<HttpResponseMessage> PostPlayersHeartBeat([Body] PlayersHeartbeatCommand players, [Query("authToken")] string authToken);
}
