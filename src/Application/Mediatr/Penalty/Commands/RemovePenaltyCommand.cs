using BanHub.Domain.Enums;
using MediatR;

namespace BanHub.Application.Mediatr.Penalty.Commands;

public class RemovePenaltyCommand : IRequest<bool>
{
    public Guid PenaltyGuid { get; set; }
    public string DeletionReason { get; set; }
    public ModifyPenalty ModifyPenalty { get; set; }
    public string? IssuerUserName { get; set; }
    public string? IssuerIdentity { get; set; }
}
