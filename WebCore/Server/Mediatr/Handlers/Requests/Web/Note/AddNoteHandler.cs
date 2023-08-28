using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Server.Mediatr.Commands.Events.Services.Discord;
using BanHub.WebCore.Server.Models.Domains;
using BanHub.WebCore.Shared.Mediatr.Commands.Requests.PlayerProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Mediatr.Handlers.Requests.Web.Note;

public class AddNoteHandler : IRequestHandler<AddNoteCommand, bool>
{
    private readonly DataContext _context;
    private readonly IMediator _mediator;

    public AddNoteHandler(DataContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<bool> Handle(AddNoteCommand request, CancellationToken cancellationToken)
    {
        var admin = await _context.Players
            .Where(x => x.Identity == request.AdminIdentity)
            .Select(x => new
            {
                x.Id,
                x.Identity,
                x.CurrentAlias.Alias.UserName
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        var target = await _context.Players
            .Where(x => x.Identity == request.TargetIdentity)
            .Select(x => new
            {
                x.Id,
                x.Identity,
                x.CurrentAlias.Alias.UserName
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);
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

        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        var message = $"**Note**: {note.NoteGuid}\n" +
                      $"**Issuer**: [{admin.UserName}](https://BanHub.gg/Players/{admin.Identity})\n" +
                      $"**Recipient**: [{target.UserName}](https://BanHub.gg/Players/{target.Identity})\n" +
                      $"**Note**: {note.Message}\n" +
                      $"**Is Private?**: {(note.IsPrivate ? "Yes" : "No")}";

        await _mediator.Publish(new CreateAdminActionNotification
        {
            Title = "Note Creation!",
            Message = message
        }, cancellationToken);

        return true;
    }
}
