using BanHubData.Commands.Heartbeat;
using RestEase;

namespace BanHub.Interfaces;

public interface IHeartbeatService
{
    [Post("/HeartBeat/Instance")]
    Task<HttpResponseMessage> PostInstanceHeartBeatAsync([Body] InstanceHeartbeatCommand instance);

    [Post("/HeartBeat/Players")]
    Task<HttpResponseMessage> PostPlayersHeartBeatAsync([Body] PlayersHeartbeatCommand players, [Query("authToken")] string authToken);
}
