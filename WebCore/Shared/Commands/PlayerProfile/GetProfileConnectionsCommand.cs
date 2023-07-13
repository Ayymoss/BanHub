using BanHub.WebCore.Shared.Models.PlayerProfileView;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.PlayerProfile;

public class GetProfileConnectionsCommand : IRequest<IEnumerable<Connection>>
{
    public string Identity { get; set; }
}
