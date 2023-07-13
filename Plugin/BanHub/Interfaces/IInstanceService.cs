using BanHubData.Commands.Instance;
using RestEase;

namespace BanHub.Interfaces;

public interface IInstanceService
{
    [Post("/Instance")]
    Task<HttpResponseMessage> CreateOrUpdateInstanceAsync([Body] CreateOrUpdateInstanceCommand instance);

    [Get("/Instance/Active/{identity}")]
    Task<HttpResponseMessage> IsInstanceActiveAsync([Path("identity")] string identity);
}
