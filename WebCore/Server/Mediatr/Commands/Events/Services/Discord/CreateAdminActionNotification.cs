using MediatR;

namespace BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Discord;

public class CreateAdminActionNotification : INotification
{
    public string Title { get; set; }
    public string Message { get; set; }
}
