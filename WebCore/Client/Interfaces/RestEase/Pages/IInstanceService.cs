using BanHub.WebCore.Shared.Commands.Instance;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase.Pages;

public interface IInstanceService
{
    [Post("/Instance/Instances")]
    Task<HttpResponseMessage> GetInstancesPaginationAsync([Body] GetInstancesPaginationCommand penaltiesPaginationQuery);

    [Get("/Instance/{identity}")]
    Task<HttpResponseMessage> GetInstanceAsync([Path("identity")] string identity);

    [Get("/Instance/Profile/Servers/{identity}")]
    Task<HttpResponseMessage> GetInstanceProfileServersAsync([Path("identity")] string identity);

    [Patch("/Instance/Activation/{identity}")]
    Task<HttpResponseMessage> ToggleInstanceActivationAsync([Path("identity")] string identity);
}
