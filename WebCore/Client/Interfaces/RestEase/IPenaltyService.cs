using BanHub.WebCore.Shared.Commands.Penalty;
using BanHubData.Commands.Penalty;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface IPenaltyService
{
    [Delete("/Penalty/Delete")]
    Task<HttpResponseMessage> DeletePenaltyAsync([Body] RemovePenaltyCommand penaltyToRemove);

    [Get("/Penalty/Profile/Penalties/{identity}")]
    Task<HttpResponseMessage> GetProfilePenaltiesAsync([Path("identity")] string identity);

    [Post("/Penalty/Pagination")]
    Task<HttpResponseMessage> GetPenaltiesPaginationAsync([Body] GetPenaltiesPaginationCommand penaltiesPaginationQuery);

    [Get("/Penalty/Latest")]
    Task<HttpResponseMessage> GetLatestBansAsync();

    [Get("/Penalty/Community/Penalties/{identity}")]
    Task<HttpResponseMessage> GetCommunityPenaltiesAsync([Path("identity")] string identity);

    [Patch("/Penalty/WebEvidence")]
    Task<HttpResponseMessage> AddPlayerPenaltyEvidenceAsync([Body] AddPlayerPenaltyEvidenceCommand evidence);
}
