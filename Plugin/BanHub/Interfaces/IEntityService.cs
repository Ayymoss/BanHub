using BanHub.Models;
using RestEase;

namespace BanHub.Interfaces;

public interface IEntityService
{
    [Get("/Entity")]
    Task<HttpResponseMessage> GetEntity([Query("identity")] string identity);

    [Post("/Entity")]
    Task<HttpResponseMessage> UpdateEntity([Body] EntityDto entities, [Query("authToken")] string authToken);

    [Post("/Entity/GetToken")]
    Task<HttpResponseMessage> GetToken([Body] EntityDto entities, [Query("authToken")] string authToken);
}
