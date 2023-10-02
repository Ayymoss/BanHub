using BanHub.Application.Mediatr.Player.Commands;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface IPlayerProfileService
{
    [Get("/Player/Profile/{identity}")]
    Task<HttpResponseMessage> GetProfileAsync([Path("identity")] string identity);

    [Post("/Player/Profile/Connections")]
    Task<HttpResponseMessage> GetProfileConnectionsPaginationAsync([Body] GetProfileConnectionsPaginationCommand pagination);

    [Post("/Penalty/Profile/Penalties")]
    Task<HttpResponseMessage> GetProfilePenaltiesPaginationAsync([Body] GetProfilePenaltiesPaginationCommand penaltiesPaginationQuery);

    [Post("/Note/Profile/Notes")]
    Task<HttpResponseMessage> GetProfileNotesPaginationAsync([Body] GetProfileNotesPaginationCommand paginationQuery);

    [Post("/Chat/Profile/Chat")]
    Task<HttpResponseMessage> GetProfileChatPaginationAsync([Body] GetProfileChatPaginationCommand paginationQuery);
}
