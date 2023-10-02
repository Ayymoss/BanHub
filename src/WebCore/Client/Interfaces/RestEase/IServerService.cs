using BanHub.Application.Mediatr.Server.Commands;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface IServerService
{
    [Post("/Server/Pagination")]
    Task<HttpResponseMessage> GetServersPaginationAsync([Body] GetServersPaginationCommand paginationQuery);
}
