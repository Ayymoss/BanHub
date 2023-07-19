using RestEase;

namespace BanHub.Interfaces;

public interface INoteService
{
    [Get("/NoteCount/{identity}")]
    Task<HttpResponseMessage> GetUserNotesCountAsync([Path("identity")] string identity);
}
