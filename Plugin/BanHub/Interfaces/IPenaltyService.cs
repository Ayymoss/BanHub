using BanHub.Models;
using RestEase;

namespace BanHub.Interfaces;

public interface IPenaltyService
{
    [Post("/Penalty")]
    Task<HttpResponseMessage> PostPenalty([Body] PenaltyDto penalty, [Query("authToken")] string authToken);

    [Post("/Penalty/Evidence")]
    Task<HttpResponseMessage> SubmitEvidence([Body] PenaltyDto evidence, [Query("authToken")] string authToken);
}
