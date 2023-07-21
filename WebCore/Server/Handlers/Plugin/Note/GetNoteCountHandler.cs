using BanHub.WebCore.Server.Context;
using BanHubData.Commands.Note;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BanHub.WebCore.Server.Handlers.Plugin.Note;

public class GetNoteCountHandler : IRequestHandler<GetNoteCountCommand, int>
{
    private readonly DataContext _context;

    public GetNoteCountHandler(DataContext context)
    {
        _context = context;
    }
    public async Task<int> Handle(GetNoteCountCommand request, CancellationToken cancellationToken)
    {
        var count = await _context.Notes.Where(x => x.Recipient.Identity == request.Identity).CountAsync(cancellationToken: cancellationToken);
        return count;
    }
}
