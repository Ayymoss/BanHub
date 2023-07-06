using Data.Enums;
using MediatR;

namespace Data.Commands.Penalty;

public class AddPlayerPenaltyEvidenceCommand : IRequest<ControllerEnums.ReturnState>
{
    public Guid PenaltyGuid { get; set; }
    public string Evidence { get; set; }
}
