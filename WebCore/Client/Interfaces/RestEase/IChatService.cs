using BanHub.WebCore.Shared.Commands.Chat;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface IChatService
{
    [Post("/Chat/Pagination")]
    Task<HttpResponseMessage> GetChatPaginationAsync([Body] GetChatPaginationCommand chatPagination);

    [Get("/Chat/Count/{identity}")]
    Task<HttpResponseMessage> GetChatCountAsync([Path("identity")] string identity);
}
