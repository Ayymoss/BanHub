using MediatR;

namespace BanHub.Application.Mediatr.Services.Events.Discord;

public class CreateAdminActionNotification : INotification
{
    public string Title { get; set; }
    public string Message { get; set; }
}
