using Data.Enums;
using Data.Commands;
using Data.Commands.Penalty;
using Data.Commands.Player;
using Data.Domains;

namespace BanHub.WebCore.Server.Interfaces;

public interface IPenaltyService
{
    Task<(ControllerEnums.ReturnState, List<Penalty>?)> GetPenaltiesAsync(string? penaltyGuid = null);
    Task<ControllerEnums.ReturnState> SubmitEvidenceAsync(AddPlayerPenaltyEvidenceCommand request);
    Task<List<Penalty>> PaginationAsync(Pagination pagination);
    Task<List<Penalty>> GetLatestThreeBansAsync();
    Task<bool> RemovePenaltyAsync(Penalty request, string requestingAdmin);
}
