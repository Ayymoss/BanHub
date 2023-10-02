using BanHub.Domain.ValueObjects;
using BanHub.Domain.ValueObjects.Plugin;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Events;

public class CreateOrUpdatePlayerNotification : CreateOrUpdatePlayerNotificationSlim, INotification
{
}
