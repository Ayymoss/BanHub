using BanHub.Domain.ValueObjects.Plugin;
using RestEase;

namespace BanHub.Plugin.Interfaces;

public interface IPenaltyService
{
    [Header("BanHubPluginVersion")] string PluginVersion { get; set; }
    [Header("BanHubApiToken")] string ApiToken { get; set; }

    [Post("/Penalty")]
    Task<HttpResponseMessage> AddPlayerPenaltyAsync([Body] AddPlayerPenaltyCommandSlim penalty);

    [Patch("/Penalty/Evidence")]
    Task<HttpResponseMessage> AddPlayerPenaltyEvidenceAsync([Body] AddPlayerPenaltyEvidenceCommandSlim evidence);
}
