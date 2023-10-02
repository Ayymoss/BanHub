using BanHub.Domain.Interfaces.Repositories;
using MediatR;

namespace BanHub.Application.Mediatr.Note.Commands;

public class GetNoteCountHandler(INoteRepository noteRepository) : IRequestHandler<GetNoteCountCommand, int>
{
    public async Task<int> Handle(GetNoteCountCommand request, CancellationToken cancellationToken)
    {
        var count = await noteRepository.GetNoteCountAsync(request.Identity, cancellationToken);
        return count;
    }
}
