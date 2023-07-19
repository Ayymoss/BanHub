using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Events.DiscordWebhook;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Note;

public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, bool>
{
    private readonly DataContext _context;

    public DeleteNoteHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _context.Notes.FirstOrDefaultAsync(x => x.NoteGuid == request.NoteGuid, cancellationToken: cancellationToken);
        if (note is null) return false;

        var noteInfo = await _context.Notes
            .Where(x => x.NoteGuid == note.NoteGuid)
            .Select(x => new
            {
                x.NoteGuid,
                AdminIdentity = x.Admin.Identity,
                AdminUserName = x.Admin.CurrentAlias.Alias.UserName,
                TargetUserName = x.Target.CurrentAlias.Alias.UserName,
                TargetIdentity = x.Target.Identity,
                x.Message,
                x.IsPrivate
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);

        var message = noteInfo is null
            ? $"Note **{note.NoteGuid}** was deleted by **{request.ActionAdminIdentity}** but no information could be found."
            : $"**Note**: {noteInfo.NoteGuid}\n" +
              $"**Admin**: {noteInfo.AdminIdentity}\n" +
              $"**Admin**: [{noteInfo.AdminUserName}](https://BanHub.gg/Players/{noteInfo.AdminIdentity})\n" +
              $"**Target**: {noteInfo.TargetIdentity}\n" +
              $"**Target**: [{noteInfo.TargetUserName}](https://BanHub.gg/Players/{noteInfo.TargetIdentity})\n" +
              $"**Note**: {noteInfo.Message}\n" +
              $"**Was Private?**: {(noteInfo.IsPrivate ? "Yes" : "No")}\n\n" +
              $"**Deleted By**: [{request.ActionAdminUserName}](https://BanHub.gg/Players/{request.ActionAdminIdentity})\n" +
              $"**Deleted For**: {request.ActionDeletionReason}";

        IDiscordWebhookSubscriptions.InvokeEvent(new CreateAdminActionEvent
        {
            Title = "Note Deletion!",
            Message = message
        }, cancellationToken);
        return true;
    }
}
