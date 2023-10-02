using MediatR;

namespace BanHub.Application.Mediatr.Services.Events.Discord;

public class CreateIssueNotification : INotification
{
    public Guid CommunityGuid { get; set; }
    public string CommunityIp { get; set; }
    public string IncomingIp { get; set; }
}
