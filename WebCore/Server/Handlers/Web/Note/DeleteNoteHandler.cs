using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Interfaces;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Note;

public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, bool>
{
    private readonly DataContext _context;
    private readonly IDiscordWebhookService _discordWebhook;

    public DeleteNoteHandler(DataContext context, IDiscordWebhookService discordWebhook)
    {
        _context = context;
        _discordWebhook = discordWebhook;
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
                TargetIdentity = x.Target.Identity,
                x.Message,
                x.IsPrivate,
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var message = noteInfo is null
            ? $"Penalty **{note.NoteGuid}** was deleted by **{request.AdminUserName}** but no information could be found."
            : $"**Penalty**: {noteInfo.NoteGuid}\n" +
              $"**Admin**: {noteInfo.AdminIdentity}\n" +
              $"**Target**: {noteInfo.TargetIdentity}\n" +
              $"**Note**: {noteInfo.Message}\n" +
              $"**Was Public?**: {(!noteInfo.IsPrivate ? "Yes" : "No")}\n\n" +
              $"**Deleted By**: {request.AdminUserName}\n" +
              $"**Deleted For**: {request.DeletionReason}";

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);
        await _discordWebhook.CreateAdminActionHookAsync("Note Deletion!", message);
        return true;
    }
}
