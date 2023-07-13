using BanHub.WebCore.Shared.Commands.Instance;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase.Pages;

public interface IInstanceService
{
    [Post("/Instance/Instances")]
    Task<HttpResponseMessage> GetInstancesPaginationAsync([Body] GetInstancesPaginationCommand penaltiesPaginationQuery);

    [Get("/Instance/{identity}")]
    Task<HttpResponseMessage> GetInstanceAsync([Query("identity")] string identity);

    [Get("/Instance/Profile/Servers/{identity}")]
    Task<HttpResponseMessage> GetInstanceServersAsync([Query("identity")] string identity);
}
