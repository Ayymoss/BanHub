using BanHub.WebCore.Shared.Models.PlayerProfileView;
using BanHub.WebCore.Shared.Models.Shared;
using MediatR;

namespace BanHub.WebCore.Shared.Commands.PlayerProfile;

public class GetProfileNotesPaginationCommand : Pagination, IRequest<PaginationContext<Note>>
{
    public bool Privileged { get; set; }
    public string Identity { get; set; }
}
