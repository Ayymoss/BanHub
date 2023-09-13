using RestEase;

namespace BanHub.Interfaces;

public interface INoteService
{
    [Header("BanHubPluginVersion")] string PluginVersion { get; set; }
    [Header("BanHubApiToken")] string ApiToken { get; set; }

    [Get("/Note/NoteCount/{identity}")]
    Task<HttpResponseMessage> GetUserNotesCountAsync([Path("identity")] string identity);
}
