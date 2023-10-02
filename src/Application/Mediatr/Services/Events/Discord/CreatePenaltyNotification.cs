using BanHub.Domain.Enums;
using MediatR;

namespace BanHub.Application.Mediatr.Services.Events.Discord;

public class CreatePenaltyNotification : INotification
{
    public PenaltyScope Scope { get; set; }
    public PenaltyType PenaltyType { get; set; }
    public Guid PenaltyGuid { get; set; }
    public string TargetIdentity { get; set; }
    public string Username { get; set; }
    public string Reason { get; set; }
    public string CommunityName { get; set; }
    public Guid CommunityGuid { get; set; }
}
