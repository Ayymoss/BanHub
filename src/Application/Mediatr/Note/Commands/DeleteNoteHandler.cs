using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Application.Mediatr.Services.Events.Discord;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Note.Commands;

public class DeleteNoteHandler(IPublisher publisher, INoteRepository noteRepository) : IRequestHandler<DeleteNoteCommand, bool>
{
    public async Task<bool> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetNoteDeleteAsync(request.NoteGuid, cancellationToken);
        if (note is null) return false;

        await noteRepository.DeleteNoteAsync(request.NoteGuid, cancellationToken);

        var message = $"**Note**: {note.NoteGuid}\n" +
                      $"**Admin**: {note.IssuerIdentity}\n" +
                      $"**Admin**: [{note.IssuerUserName}](https://BanHub.gg/Players/{note.IssuerIdentity})\n" +
                      $"**Target**: {note.RecipientIdentity}\n" +
                      $"**Target**: [{note.RecipientUserName}](https://BanHub.gg/Players/{note.RecipientIdentity})\n" +
                      $"**Note**: {note.Message}\n" +
                      $"**Was Private?**: {(note.IsPrivate ? "Yes" : "No")}\n\n" +
                      $"**Deleted By**: [{request.IssuerUserName}](https://BanHub.gg/Players/{request.IssuerIdentity})\n" +
                      $"**Deleted For**: {request.DeletionReason}";

        await publisher.Publish(new CreateAdminActionNotification
        {
            Title = "Note Deletion!",
            Message = message
        }, cancellationToken);

        return true;
    }
}
