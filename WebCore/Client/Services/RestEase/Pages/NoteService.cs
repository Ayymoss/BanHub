using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Client.Utilities;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class NoteService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly INoteService _api = RestClient.For<INoteService>(ApiHost);

    public async Task<bool> AddNoteAsync(AddNoteCommand noteToAdd)
    {
        try
        {
            var response = await _api.AddNoteAsync(noteToAdd);
            return response.IsSuccessStatusCode;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to add note: {e.Message}");
        }

        return false;
    }

    public async Task<bool> DeleteNoteAsync(DeleteNoteCommand noteToRemove)
    {
        try
        {
            var response = await _api.RemoveNoteAsync(noteToRemove);
            return response.IsSuccessStatusCode;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to remove note: {e.Message}");
        }

        return false;
    }

    public async Task<IEnumerable<Note>> GetNotesAsync(string identity)
    {
        try
        {
            var response = await _api.GetNotesAsync(identity);
            var result = await response.DeserializeHttpResponseContentAsync<IEnumerable<Note>>();
            return result ?? new List<Note>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player notes: {e.Message}");
        }

        return new List<Note>();
    }
}
