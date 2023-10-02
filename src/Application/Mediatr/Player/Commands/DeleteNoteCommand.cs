using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class DeleteNoteCommand : IRequest<bool>
{
    public Guid NoteGuid { get; set; }
    public string? IssuerUserName { get; set; }
    public string? IssuerIdentity { get; set; }
    public string? DeletionReason { get; set; }
}
