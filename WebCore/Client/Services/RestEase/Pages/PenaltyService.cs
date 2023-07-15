using BanHub.WebCore.Client.Interfaces.RestEase.Pages;
using BanHub.WebCore.Shared.Commands.Penalty;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Models.Shared;
using BanHub.WebCore.Shared.Utilities;
using BanHubData.Commands.Penalty;
using RestEase;

namespace BanHub.WebCore.Client.Services.RestEase.Pages;

public class PenaltyService
{
#if DEBUG
    private const string ApiHost = "http://localhost:8123/api";
#else
    private const string ApiHost = "http://banhub.gg:8123/api";
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

    public async Task<IEnumerable<WebCore.Shared.Models.PenaltiesView.Penalty>> GetPenaltiesPaginationAsync(
        GetPenaltiesPaginationCommand paginationQuery)
    {
        try
        {
            var response = await _api.GetPenaltiesPaginationAsync(paginationQuery);
            var result = await response.DeserializeHttpResponseContentAsync<IEnumerable<WebCore.Shared.Models.PenaltiesView.Penalty>>();
            return result ?? new List<WebCore.Shared.Models.PenaltiesView.Penalty>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get penalties: {e.Message}");
        }

        return new List<WebCore.Shared.Models.PenaltiesView.Penalty>();
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

    public async Task<IEnumerable<WebCore.Shared.Models.InstanceProfileView.Penalty>> GetInstancePenaltiesAsync(string identity)
    {
        try
        {
            var response = await _api.GetInstancePenaltiesAsync(identity);
            var result =
                await response.DeserializeHttpResponseContentAsync<IEnumerable<WebCore.Shared.Models.InstanceProfileView.Penalty>>();
            return result ?? new List<WebCore.Shared.Models.InstanceProfileView.Penalty>();
        }
        catch (ApiException e)
        {
            Console.WriteLine($"API->Failed to get player penalties: {e.Message}");
        }

        return new List<WebCore.Shared.Models.InstanceProfileView.Penalty>();
    }
}
