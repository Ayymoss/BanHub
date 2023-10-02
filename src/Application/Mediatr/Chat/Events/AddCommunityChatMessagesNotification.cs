using BanHub.Domain.ValueObjects.Plugin;
using MediatR;

namespace BanHub.Application.Mediatr.Chat.Events;

public class AddCommunityChatMessagesNotification : AddCommunityChatMessagesNotificationSlim, INotification
{
}
