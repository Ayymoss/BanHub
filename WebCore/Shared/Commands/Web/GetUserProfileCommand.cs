using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.Web;

public class GetUserProfileCommand : IRequest<WebUser?>
{
    public int UserId { get; set; }
}
