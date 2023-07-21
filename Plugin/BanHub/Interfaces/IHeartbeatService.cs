using BanHubData.Commands.Heartbeat;
using RestEase;

namespace BanHub.Interfaces;

public interface IHeartbeatService
{
    [Post("/HeartBeat/Community")]
    Task<HttpResponseMessage> PostCommunityHeartBeatAsync([Body] CommunityHeartbeatCommand community);

    [Post("/HeartBeat/Players")]
    Task<HttpResponseMessage> PostPlayersHeartBeatAsync([Body] PlayersHeartbeatCommand players, [Query("authToken")] string authToken);
}
