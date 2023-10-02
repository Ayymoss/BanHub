using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetUserProfileCommand : IRequest<WebUser?>
{
    public string SignedInGuid { get; set; }
}
