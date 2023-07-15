using BanHub.WebCore.Shared.Commands.Penalty;
using BanHubData.Commands.Penalty;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase.Pages;

public interface IPenaltyService
{
    [Delete("/Penalty/Delete")]
    Task<HttpResponseMessage> DeletePenaltyAsync([Body] RemovePenaltyCommand penaltyToRemove);

    [Get("/Penalty/Profile/Penalties/{identity}")]
    Task<HttpResponseMessage> GetProfilePenaltiesAsync([Path("identity")] string identity);

    [Post("/Penalty/Penalties")]
    Task<HttpResponseMessage> GetPenaltiesPaginationAsync([Body] GetPenaltiesPaginationCommand penaltiesPaginationQuery);

    [Get("/Penalty/Latest")]
    Task<HttpResponseMessage> GetLatestBansAsync();

    [Get("/Penalty/Instance/Penalties/{identity}")]
    Task<HttpResponseMessage> GetInstancePenaltiesAsync([Path("identity")] string identity);

    [Patch("/Penalty/WebEvidence")]
    Task<HttpResponseMessage> AddPlayerPenaltyEvidenceAsync([Body] AddPlayerPenaltyEvidenceCommand evidence);
}
