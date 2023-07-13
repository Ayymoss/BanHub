using MediatR;

namespace BanHub.WebCore.Shared.Commands.PlayerProfile;

public class DeleteNoteCommand : IRequest<bool>
{
    public Guid NoteGuid { get; set; }
    public string AdminUserName { get; set; }
    public string DeletionReason { get; set; }
}
