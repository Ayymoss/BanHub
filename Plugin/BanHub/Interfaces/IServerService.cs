using BanHub.Models;
using RestEase;

namespace BanHub.Interfaces;

public interface IServerService
{
    [Post("/Server")]
    Task<HttpResponseMessage> PostServer([Body] ServerDto server, [Query("authToken")] string authToken);
}
