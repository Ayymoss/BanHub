using MediatR;

namespace BanHub.WebCore.Shared.Commands.PlayerProfile;

public class DeleteNoteCommand : IRequest<bool>
{
    public Guid NoteGuid { get; set; }
    public string? ActionAdminUserName { get; set; }
    public string? ActionAdminIdentity { get; set; }
    public string? ActionDeletionReason { get; set; }
}
