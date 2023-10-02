using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Application.Mediatr.Services.Events.Discord;
using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Note.Commands;

public class AddNoteHandler(IPublisher publisher, IPlayerRepository playerRepository, INoteRepository noteRepository)
    : IRequestHandler<AddNoteCommand, bool>
{
    public async Task<bool> Handle(AddNoteCommand request, CancellationToken cancellationToken)
    {
        if (request.AdminIdentity is null) return false;

        var admin = await playerRepository.GetPlayerIdAsync(request.AdminIdentity, cancellationToken);
        var target = await playerRepository.GetPlayerIdAsync(request.TargetIdentity, cancellationToken);

        if (admin is null || target is null) return false;

        var note = new EFNote
        {
            NoteGuid = Guid.NewGuid(),
            RecipientId = target.Id,
            IssuerId = admin.Id,
            Message = request.Message,
            IsPrivate = request.IsPrivate,
            Created = DateTimeOffset.UtcNow
        };

        await noteRepository.CreateNoteAsync(note, cancellationToken);

        var message = $"**Note**: {note.NoteGuid}\n" +
                      $"**Issuer**: [{admin.UserName}](https://BanHub.gg/Players/{admin.Identity})\n" +
                      $"**Recipient**: [{target.UserName}](https://BanHub.gg/Players/{target.Identity})\n" +
                      $"**Note**: {note.Message}\n" +
                      $"**Is Private?**: {(note.IsPrivate ? "Yes" : "No")}";

        await publisher.Publish(new CreateAdminActionNotification
        {
            Title = "Note Creation!",
            Message = message
        }, cancellationToken);

        return true;
    }
}
