using BanHubData.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace BanHub.WebCore.Server.SignalR;

public class PluginHub : Hub
{
    public async Task BroadcastGlobalBanAsync(int count)
    {
        await Clients.All.SendAsync(HubMethods.OnGlobalBan, count);
    }
}
