using BanHub.Models;
using RestEase;

namespace BanHub.Interfaces;

public interface IHeartBeatService
{
    [Post("/HeartBeat/Instance")]
    Task<HttpResponseMessage> PostInstanceHeartBeat([Body] InstanceDto instance);

    [Post("/HeartBeat/Entities")]
    Task<HttpResponseMessage> PostEntityHeartBeat([Body] List<EntityDto> entities, [Query("authToken")] string authToken);
}
