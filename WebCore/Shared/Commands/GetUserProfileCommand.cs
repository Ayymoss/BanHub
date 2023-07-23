using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands;

public class GetUserProfileCommand : IRequest<WebUser?>
{
    public string SignedInGuid { get; set; }
}
