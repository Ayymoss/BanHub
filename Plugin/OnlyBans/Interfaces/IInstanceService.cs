using Data.Domains;
using RestEase;

namespace BanHub.Interfaces;

public interface IInstanceService
{
    [Post("/Instance")]
    Task<HttpResponseMessage> PostInstance([Body] Instance instance);

    [Get("/Instance/Active")]
    Task<HttpResponseMessage> IsInstanceActive([Query("guid")] string guid);
}
