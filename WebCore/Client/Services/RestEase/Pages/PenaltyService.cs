using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Penalty;
using BanHub.WebCore.Shared.Models.Shared;
using BanHub.WebCore.Client.Utilities;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHubData.Mediatr.Commands.Requests.Penalty;
using RestEase;
using Penalty = BanHub.WebCore.Shared.Models.PlayerProfileView.Penalty;

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
    
    public async Task<PaginationContext<BanHub.WebCore.Shared.Models.PenaltiesView.Penalty>> GetPenaltiesPaginationAsync(
        GetPenaltiesPaginationCommand paginationQuery)
    {
        try
        {
            var response = await _api.GetPenaltiesPaginationAsync(paginationQuery);
            var result = await response.DeserializeHttpResponseContentAsync<PaginationContext<BanHub.WebCore.Shared.Models.PenaltiesView.Penalty>>();
            return result ?? new PaginationContext<BanHub.WebCore.Shared.Models.PenaltiesView.Penalty>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get penalties: {e.Message}");
        }

        return new PaginationContext<BanHub.WebCore.Shared.Models.PenaltiesView.Penalty>();
    }

    public async Task<IEnumerable<WebCore.Shared.Models.IndexView.Penalty>> GetLatestBansAsync()
    {
        try
        {
            var response = await _api.GetLatestBansAsync();
            var result = await response.DeserializeHttpResponseContentAsync<IEnumerable<WebCore.Shared.Models.IndexView.Penalty>>();
            return result ?? new List<WebCore.Shared.Models.IndexView.Penalty>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get penalties: {e.Message}");
        }

        return new List<WebCore.Shared.Models.IndexView.Penalty>();
    }

   
}
