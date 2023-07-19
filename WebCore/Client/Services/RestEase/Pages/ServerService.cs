using BanHub.WebCore.Client.Interfaces.RestEase.Pages;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class ServerService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly IServerService _api;

    public ServerService()
    {
        _api = RestClient.For<IServerService>(ApiHost);
    }

    
}
