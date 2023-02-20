using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IPenaltyService
{
    Task<(ControllerEnums.ReturnState, Guid?)> AddPenaltyAsync(PenaltyDto request);
    Task<(ControllerEnums.ReturnState, PenaltyDto?)> GetPenaltyAsync(string penaltyGuid);
    Task<(ControllerEnums.ReturnState, List<PenaltyDto>?)> GetPenaltiesAsync();
    Task<bool> SubmitEvidenceAsync(PenaltyDto request);
    Task<List<PenaltyDto>> PaginationAsync(PaginationDto pagination);
    Task<List<PenaltyDto>> GetLatestThreeBansAsync();
    Task<bool> RemovePenaltyAsync(PenaltyDto request, string requestingAdmin);
}
