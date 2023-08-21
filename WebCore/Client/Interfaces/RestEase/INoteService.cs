using BanHub.WebCore.Shared.Commands.PlayerProfile;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface INoteService
{
    [Post("/Note")]
    Task<HttpResponseMessage> AddNoteAsync([Body] AddNoteCommand noteToAdd);

    [Delete("/Note")]
    Task<HttpResponseMessage> RemoveNoteAsync([Body] DeleteNoteCommand noteToRemove);
    
}
