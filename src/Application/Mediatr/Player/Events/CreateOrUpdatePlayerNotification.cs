using BanHub.Domain.ValueObjects;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Events;

public class CreateOrUpdatePlayerNotification : CreateOrUpdatePlayerNotificationSlim, INotification
{
}
