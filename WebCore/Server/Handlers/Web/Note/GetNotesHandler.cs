using BanHub.WebCore.Server.Context;
using BanHub.WebCore.Shared.Commands.PlayerProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Web.Note;

public class GetNotesHandler : IRequestHandler<GetNotesCommand, IEnumerable<Shared.Models.PlayerProfileView.Note>>
{
    private readonly DataContext _context;

    public GetNotesHandler(DataContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shared.Models.PlayerProfileView.Note>> Handle(GetNotesCommand request,
        CancellationToken cancellationToken)
    {
        var notes = await _context.Notes.Where(x => x.Target.Identity == request.Identity).Select(x =>
            new Shared.Models.PlayerProfileView.Note
            {
                NoteGuid = x.NoteGuid,
                Message = x.Message,
                AdminUserName = x.Admin.CurrentAlias.Alias.UserName,
                Created = x.Created,
                IsPrivate = x.IsPrivate
            }).ToListAsync(cancellationToken: cancellationToken);

        return notes;
    }
}
