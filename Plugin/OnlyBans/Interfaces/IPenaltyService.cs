using Data.Domains;
using RestEase;

namespace BanHub.Interfaces;

public interface IPenaltyService
{
    [Post("/Penalty")]
    Task<HttpResponseMessage> PostPenalty([Body] Penalty penalty, [Query("authToken")] string authToken);

    [Post("/Penalty/Evidence")]
    Task<HttpResponseMessage> SubmitEvidence([Body] Penalty evidence, [Query("authToken")] string authToken);
}
