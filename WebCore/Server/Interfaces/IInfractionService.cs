using BanHub.WebCore.Server.Enums;
using BanHub.WebCore.Shared.DTOs;

namespace BanHub.WebCore.Server.Interfaces;

public interface IInfractionService
{
    Task<(ControllerEnums.ProfileReturnState, Guid?)> AddInfraction(InfractionDto request);
    Task<(ControllerEnums.ProfileReturnState, InfractionDto?)> GetInfraction(string infractionGuid);
    Task<(ControllerEnums.ProfileReturnState, List<InfractionDto>?)> GetInfractions();
    Task<int> GetInfractionDayCount();
    Task<bool> SubmitEvidence(InfractionDto request);
}
