using BanHubData.Commands.Penalty;
using RestEase;

namespace BanHub.Interfaces;

public interface IPenaltyService
{
    [Post("/Penalty")]
    Task<HttpResponseMessage> AddPlayerPenaltyAsync([Query("authToken")] string authToken, [Body] AddPlayerPenaltyCommand penalty);

    [Patch("/Penalty/Evidence")]
    Task<HttpResponseMessage> AddPlayerPenaltyEvidenceAsync([Query("authToken")] string authToken, [Body] AddPlayerPenaltyEvidenceCommand evidence);
}
