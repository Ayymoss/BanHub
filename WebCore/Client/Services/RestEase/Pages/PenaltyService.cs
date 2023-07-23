using BanHub.WebCore.Client.Interfaces.RestEase;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHub.WebCore.Shared.Models.PenaltiesView;
using BanHub.WebCore.Shared.Utilities;
using BanHubData.Commands.Penalty;
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
    private readonly IPenaltyService _api;

    public PenaltyService()
    {
        _api = RestClient.For<IPenaltyService>(ApiHost);
    }

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

    public async Task<IEnumerable<Penalty>> GetProfilePenaltiesAsync(string identity)
    {
        try
        {
            var response = await _api.GetProfilePenaltiesAsync(identity);
            var result = await response.DeserializeHttpResponseContentAsync<IEnumerable<Penalty>>();
            return result ?? new List<Penalty>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player penalties: {e.Message}");
        }

        return new List<Penalty>();
    }

    public async Task<PenaltyContext> GetPenaltiesPaginationAsync(
        GetPenaltiesPaginationCommand paginationQuery)
    {
        try
        {
            var response = await _api.GetPenaltiesPaginationAsync(paginationQuery);
            var result = await response.DeserializeHttpResponseContentAsync<PenaltyContext>();
            return result ?? new PenaltyContext();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get penalties: {e.Message}");
        }

        return new PenaltyContext();
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

    public async Task<IEnumerable<WebCore.Shared.Models.CommunityProfileView.Penalty>> GetCommunityPenaltiesAsync(string identity)
    {
        try
        {
            var response = await _api.GetCommunityPenaltiesAsync(identity);
            var result =
                await response.DeserializeHttpResponseContentAsync<IEnumerable<WebCore.Shared.Models.CommunityProfileView.Penalty>>();
            return result ?? new List<WebCore.Shared.Models.CommunityProfileView.Penalty>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player penalties: {e.Message}");
        }

        return new List<WebCore.Shared.Models.CommunityProfileView.Penalty>();
    }
}
