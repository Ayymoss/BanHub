using RestEase;

namespace BanHub.Interfaces;

public interface INoteService
{
    [Get("/Note/NoteCount/{identity}")]
    Task<HttpResponseMessage> GetUserNotesCountAsync([Path("identity")] string identity);
}
