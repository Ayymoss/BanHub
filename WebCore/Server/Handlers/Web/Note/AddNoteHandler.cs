using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Models.Domains;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Note;

public class AddNoteHandler : IRequestHandler<AddNoteCommand, bool>
{
    private readonly DataContext _context;

    public AddNoteHandler(DataContext context)
    {
        _context = context;
    }
    public async Task<bool> Handle(AddNoteCommand request, CancellationToken cancellationToken)
    {
        var admin = await _context.Players.FirstOrDefaultAsync(x => x.Identity == request.AdminIdentity, cancellationToken: cancellationToken);
        var target = await _context.Players.FirstOrDefaultAsync(x => x.Identity == request.TargetIdentity, cancellationToken: cancellationToken);
        if (admin is null || target is null) return false;

        var note = new EFNote
        {
            NoteGuid = Guid.NewGuid(),
            TargetId = target.Id,
            AdminId = admin.Id,
            Message = request.Message,
            IsPrivate = request.IsPrivate,
            Created = DateTimeOffset.UtcNow
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
