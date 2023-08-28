using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;

public class DeleteNoteCommand : IRequest<bool>
{
    public Guid NoteGuid { get; set; }
    public string? IssuerUserName { get; set; }
    public string? IssuerIdentity { get; set; }
    public string? DeletionReason { get; set; }
}
