using BanHubData.Mediatr.Commands.Requests.Penalty;
using RestEase;

namespace BanHub.Interfaces;

public interface IPenaltyService
{
    [Header("BanHubPluginVersion")] string PluginVersion { get; set; }
    [Header("BanHubApiToken")] string ApiToken { get; set; }

    [Post("/Penalty")]
    Task<HttpResponseMessage> AddPlayerPenaltyAsync([Body] AddPlayerPenaltyCommand penalty);

    [Patch("/Penalty/Evidence")]
    Task<HttpResponseMessage> AddPlayerPenaltyEvidenceAsync([Body] AddPlayerPenaltyEvidenceCommand evidence);
}
