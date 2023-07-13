using BanHubData.Commands.Instance;
using RestEase;

namespace BanHub.Interfaces;

public interface IInstanceService
{
    [Post("/Instance")]
    Task<HttpResponseMessage> PostInstance([Body] CreateOrUpdateInstanceCommand instance);

    [Get("/Instance/Active")]
    Task<HttpResponseMessage> IsInstanceActive([Body("guid")] IsInstanceActiveCommand guid); // TODO: Validate!
}
