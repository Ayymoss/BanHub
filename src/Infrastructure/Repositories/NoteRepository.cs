using BanHub.Domain.Entities;
using BanHub.Domain.Interfaces.Repositories;
using BanHub.Domain.ValueObjects.Repository.Note;
using BanHub.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BanHub.Infrastructure.Repositories;

public class NoteRepository(DataContext context) : INoteRepository
{
    public async Task<int> GetNoteCountAsync(string identity, CancellationToken cancellationToken)
    {
        var count = await context.Notes
            .Where(x => x.Recipient.Identity == identity)
            .CountAsync(cancellationToken: cancellationToken);
        return count;
    }

    public async Task<NoteDelete?> GetNoteDeleteAsync(Guid noteGuid, CancellationToken cancellationToken)
    {
        var note = await context.Notes
            .Where(x => x.NoteGuid == noteGuid)
            .Select(x => new NoteDelete
            {
                NoteGuid = x.NoteGuid,
                IssuerIdentity = x.Issuer.Identity,
                IssuerUserName = x.Issuer.CurrentAlias.Alias.UserName,
                RecipientUserName = x.Recipient.CurrentAlias.Alias.UserName,
                RecipientIdentity = x.Recipient.Identity,
                Message = x.Message,
                IsPrivate = x.IsPrivate
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return note;
    }

    public async Task DeleteNoteAsync(Guid noteGuid, CancellationToken cancellationToken)
    {
        var note = await context.Notes
            .Where(x => x.NoteGuid == noteGuid)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (note is null) return;
        context.Notes.Remove(note);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateNoteAsync(EFNote note, CancellationToken cancellationToken)
    {
        context.Notes.Add(note);
        await context.SaveChangesAsync(cancellationToken);
    }
}
