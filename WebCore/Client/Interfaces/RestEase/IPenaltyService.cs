using BanHub.WebCore.Shared.Mediatr.Commands.Requests.Penalty;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using BanHubData.Mediatr.Commands.Requests.Penalty;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase;

public interface IPenaltyService
{
    [Delete("/Penalty/Delete")]
    Task<HttpResponseMessage> DeletePenaltyAsync([Body] RemovePenaltyCommand penaltyToRemove);

    [Post("/Penalty/Pagination")]
    Task<HttpResponseMessage> GetPenaltiesPaginationAsync([Body] GetPenaltiesPaginationCommand penaltiesPaginationQuery);

    [Get("/Penalty/Latest")]
    Task<HttpResponseMessage> GetLatestBansAsync();

    [Patch("/Penalty/WebEvidence")]
    Task<HttpResponseMessage> AddPlayerPenaltyEvidenceAsync([Body] AddPlayerPenaltyEvidenceCommand evidence);
}
