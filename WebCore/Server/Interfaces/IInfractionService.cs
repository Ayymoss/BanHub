using GlobalInfraction.WebCore.Server.Enums;
using GlobalInfraction.WebCore.Shared.Models;

namespace GlobalInfraction.WebCore.Server.Interfaces;

public interface IInfractionService
{
    Task<(ControllerEnums.ProfileReturnState, Guid?)> AddInfraction(InfractionDto request);
}
