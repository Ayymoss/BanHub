using BanHubData.Commands.Instance.Server;
using RestEase;

namespace BanHub.Interfaces;

public interface IServerService
{
    [Post("/Server")]
    Task<HttpResponseMessage> PostServer([Query("authToken")] string authToken, [Body] CreateOrUpdateServerCommand server);
}
