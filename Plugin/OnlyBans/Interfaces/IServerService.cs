using Data.Domains;
using RestEase;

namespace BanHub.Interfaces;

public interface IServerService
{
    [Post("/Server")]
    Task<HttpResponseMessage> PostServer([Body] Server server, [Query("authToken")] string authToken);
}
