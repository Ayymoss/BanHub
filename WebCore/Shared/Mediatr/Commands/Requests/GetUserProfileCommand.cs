using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests;

public class GetUserProfileCommand : IRequest<WebUser?>
{
    public string SignedInGuid { get; set; }
}
