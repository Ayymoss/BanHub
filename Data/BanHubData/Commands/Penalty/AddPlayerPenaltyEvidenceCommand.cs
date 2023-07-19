using BanHubData.Enums;
using MediatR;

namespace BanHubData.Commands.Penalty;

public class AddPlayerPenaltyEvidenceCommand : IRequest<ControllerEnums.ReturnState>
{
    public Guid PenaltyGuid { get; set; }
    public string Evidence { get; set; }
    public string OffenderUsername { get; set; }
    public string OffenderIdentity { get; set; }
    public string IssuerIdentity { get; set; }
    public string IssuerUsername { get; set; }
}
