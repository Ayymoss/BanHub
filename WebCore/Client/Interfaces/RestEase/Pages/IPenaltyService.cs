using BanHub.WebCore.Shared.Commands.Penalty;
using RestEase;

namespace BanHub.WebCore.Client.Interfaces.RestEase.Pages;

public interface IPenaltyService
{
    [Delete("/Penalty/Delete")]
    Task<HttpResponseMessage> RemovePenaltyAsync([Body] RemovePenaltyCommand penaltyToRemove);

    [Get("/Penalty/Penalties/{identity}")]
    Task<HttpResponseMessage> GetProfilePenaltiesAsync([Query("identity")] string identity);

    [Post("/Penalty/Penalties")]
    Task<HttpResponseMessage> GetPenaltiesPaginationAsync([Body] GetPenaltiesPaginationCommand penaltiesPaginationQuery);

    [Get("/Penalty/Latest")]
    Task<HttpResponseMessage> GetLatestBansAsync();
}
