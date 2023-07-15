using BanHubData.Enums;
using MediatR;

namespace BanHubData.Commands.Penalty;

public class AddPlayerPenaltyEvidenceCommand : IRequest<ControllerEnums.ReturnState>
{
    public Guid PenaltyGuid { get; set; }
    public string Evidence { get; set; }
    public string? ActionAdminUserName { get; set; }
    public string? ActionAdminIdentity { get; set; }
}
