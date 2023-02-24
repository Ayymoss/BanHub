using BanHub.Models;
using RestEase;

namespace BanHub.Interfaces;

public interface IInstanceService
{
    [Post("/Instance")]
    Task<HttpResponseMessage> PostInstance([Body] InstanceDto instance);

    [Get("/Instance/Active")]
    Task<HttpResponseMessage> IsInstanceActive([Query("guid")] string guid);
}
