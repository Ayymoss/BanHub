using BanHub.Application.Mediatr.Penalty.Commands;
using BanHub.Domain.ValueObjects.Services;
using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Client.Utilities;
using RestEase;
using Penalty = BanHub.Application.DTOs.WebView.PlayerProfileView.Penalty;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class PenaltyService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "https://banhub.gg/api";
#endif
    private readonly IPenaltyService _api = RestClient.For<IPenaltyService>(ApiHost);

    public async Task<bool> DeletePenaltyAsync(RemovePenaltyCommand penaltyToRemove)
    {
        try
        {
            var response = await _api.DeletePenaltyAsync(penaltyToRemove);
            return response.IsSuccessStatusCode;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player profile: {e.Message}");
        }

        return false;
    }

    public async Task<bool> AddPlayerPenaltyEvidenceAsync(AddPlayerPenaltyEvidenceCommand evidence)
    {
        try
        {
            var response = await _api.AddPlayerPenaltyEvidenceAsync(evidence);
            return response.IsSuccessStatusCode;
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player profile: {e.Message}");
        }

        return false;
    }
    
    public async Task<PaginationContext<Application.DTOs.WebView.PenaltiesView.Penalty>> GetPenaltiesPaginationAsync(
        GetPenaltiesPaginationCommand paginationQuery)
    {
        try
        {
            var response = await _api.GetPenaltiesPaginationAsync(paginationQuery);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<Application.DTOs.WebView.PenaltiesView.Penalty>>();
            return result ?? new PaginationContext<Application.DTOs.WebView.PenaltiesView.Penalty>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get penalties: {e.Message}");
        }

        return new PaginationContext<Application.DTOs.WebView.PenaltiesView.Penalty>();
    }

    public async Task<IEnumerable<Application.DTOs.WebView.IndexView.Penalty>> GetLatestBansAsync()
    {
        try
        {
            var response = await _api.GetLatestBansAsync();
            var result = await response.DeserializeHttpResponseContentAsync<IEnumerable<Application.DTOs.WebView.IndexView.Penalty>>();
            return result ?? new List<Application.DTOs.WebView.IndexView.Penalty>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get penalties: {e.Message}");
        }

        return new List<Application.DTOs.WebView.IndexView.Penalty>();
    }

   
}
