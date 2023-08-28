using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Discord;

public class CreateIssueNotification : INotification
{
    public Guid CommunityGuid { get; set; }
    public string CommunityIp { get; set; }
    public string IncomingIp { get; set; }
}
