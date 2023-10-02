using BanHub.Application.Mediatr.Chat.Commands;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface IChatService
{
    [Get("/Chat/Count/{identity}")]
    Task<HttpResponseMessage> GetChatCountAsync([Path("identity")] string identity);

    [Post("/Chat/Context")]
    Task<HttpResponseMessage> GetChatContextAsync([Body] GetMessageContextCommand chatMessageContext);
}
