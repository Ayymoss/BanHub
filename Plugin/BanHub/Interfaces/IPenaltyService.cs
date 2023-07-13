using BanHubData.Commands.Penalty;
using RestEase;

namespace BanHub.Interfaces;

public interface IPenaltyService
{
    [Post("/Penalty")]
    Task<HttpResponseMessage> PostPenalty([Query("authToken")] string authToken, [Body] AddPlayerPenaltyCommand penalty);

    [Post("/Penalty/Evidence")]
    Task<HttpResponseMessage> SubmitEvidence([Query("authToken")] string authToken, [Body] AddPlayerPenaltyEvidenceCommand evidence);
}
