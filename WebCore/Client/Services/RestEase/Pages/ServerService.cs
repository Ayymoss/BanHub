using BanHub.WebCore.Client.Interfaces.RestEase.Pages;
using BanHub.WebCore.Shared.Models.InstanceProfileView;
using BanHub.WebCore.Shared.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class ServerService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "http://banhub.gg:8123/api";
#endif
    private readonly IServerService _api;

    public ServerService()
    {
        _api = RestClient.For<IServerService>(ApiHost);
    }

    
}
