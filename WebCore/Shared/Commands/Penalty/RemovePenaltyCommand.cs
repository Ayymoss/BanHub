using MediatR;

namespace BanHub.WebCore.Shared.Commands.Penalty;

public class RemovePenaltyCommand : IRequest<bool>
{
    public Guid PenaltyGuid { get; set; }
    public string DeletionReason { get; set; }
    public string? AdminUserName { get; set; }
    public string? ActionAdminIdentity { get; set; }
}
