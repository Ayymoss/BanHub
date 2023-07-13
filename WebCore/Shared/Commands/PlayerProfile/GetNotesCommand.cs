using BanHub.WebCore.Shared.Models.PlayerProfileView;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.PlayerProfile;

public class GetNotesCommand : IRequest<IEnumerable<Note>>
{
    public string Identity { get; set; }
    public bool Authorised { get; set; }
}
