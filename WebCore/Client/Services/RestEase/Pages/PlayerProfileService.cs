using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Client.Utilities;
using BanHub.WebCore.Shared.Commands.Chat;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.Shared;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;


public class PlayerProfileService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly IPlayerProfileService _api = RestClient.For<IPlayerProfileService>(ApiHost);

    public async Task<Player> GetProfileAsync(string identity)
    {
        try
        {
            var response = await _api.GetProfileAsync(identity);
            var result = await response.DeserializeHttpResponseContentAsync<Player>();
            return result ?? new Player();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player profile: {e.Message}");
        }

        return new Player();
    }

    public async Task<PaginationContext<Connection>> GetProfileConnectionsPaginationAsync(GetProfileConnectionsPaginationCommand pagination)
    {
        try
        {
            var response = await _api.GetProfileConnectionsPaginationAsync(pagination);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<Connection>>();
            return result ?? new PaginationContext<Connection>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player connections: {e.Message}");
        }

        return new PaginationContext<Connection>();
    }

    public async Task<PaginationContext<Penalty>> GetProfilePenaltiesPaginationAsync(GetProfilePenaltiesPaginationCommand paginationQuery)
    {
        try
        {
            var response = await _api.GetProfilePenaltiesPaginationAsync(paginationQuery);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<Penalty>>();
            return result ?? new PaginationContext<Penalty>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player penalties: {e.Message}");
        }

        return new PaginationContext<Penalty>();
    }
    
    public async Task<PaginationContext<Note>> GetProfileNotesPaginationAsync(GetProfileNotesPaginationCommand paginationQuery)
    {
        try
        {
            var response = await _api.GetProfileNotesPaginationAsync(paginationQuery);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<Note>>();
            return result ?? new PaginationContext<Note>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player penalties: {e.Message}");
        }

        return new PaginationContext<Note>();
    }
    public async Task<PaginationContext<Chat>> GetProfileChatPaginationAsync(GetProfileChatPaginationCommand paginationQuery)
    {
        try
        {
            var response = await _api.GetProfileChatPaginationAsync(paginationQuery);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<Chat>>();
            return result ?? new PaginationContext<Chat>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get chat: {e.Message}");
        }

        return new PaginationContext<Chat>();
    }

    
    
}
