﻿using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Discord;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Note;

public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, bool>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;

    public DeleteNoteHandler(DataContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
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
                IssuerIdentity = x.Issuer.Identity,
                IssuerUserName = x.Issuer.CurrentAlias.Alias.UserName,
                RecipientUserName = x.Recipient.CurrentAlias.Alias.UserName,
                RecipientIdentity = x.Recipient.Identity,
                x.Message,
                x.IsPrivate
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);

        var message = noteInfo is null
            ? $"Note **{note.NoteGuid}** was deleted by **{request.IssuerIdentity}** but no information could be found."
            : $"**Note**: {noteInfo.NoteGuid}\n" +
              $"**Admin**: {noteInfo.IssuerIdentity}\n" +
              $"**Admin**: [{noteInfo.IssuerUserName}](https://BanHub.gg/Players/{noteInfo.IssuerIdentity})\n" +
              $"**Target**: {noteInfo.RecipientIdentity}\n" +
              $"**Target**: [{noteInfo.RecipientUserName}](https://BanHub.gg/Players/{noteInfo.RecipientIdentity})\n" +
              $"**Note**: {noteInfo.Message}\n" +
              $"**Was Private?**: {(noteInfo.IsPrivate ? "Yes" : "No")}\n\n" +
              $"**Deleted By**: [{request.IssuerUserName}](https://BanHub.gg/Players/{request.IssuerIdentity})\n" +
              $"**Deleted For**: {request.DeletionReason}";

        await _mediator.Publish(new CreateAdminActionNotification
        {
            Title = "Note Deletion!",
            Message = message
        }, cancellationToken);

        return true;
    }
}