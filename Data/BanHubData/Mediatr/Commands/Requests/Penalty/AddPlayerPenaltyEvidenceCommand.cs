using BanHubData.Enums;
using MediatR;

namespace BanHubData.Mediatr.Commands.Requests.Penalty;

public class AddPlayerPenaltyEvidenceCommand : IRequest<ControllerEnums.ReturnState>
{
    public Guid PenaltyGuid { get; set; }
    public string Evidence { get; set; }
    public string? IssuerIdentity { get; set; }
    public string? IssuerUsername { get; set; }
}
