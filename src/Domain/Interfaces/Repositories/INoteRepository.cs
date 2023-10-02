using BanHub.Domain.Entities;
using BanHub.Domain.ValueObjects.Repository.Note;

namespace BanHub.Domain.Interfaces.Repositories;

public interface INoteRepository
{
    Task<int> GetNoteCountAsync(string identity, CancellationToken cancellationToken);
    Task<NoteDelete?> GetNoteDeleteAsync(Guid noteGuid, CancellationToken cancellationToken);
    Task DeleteNoteAsync(Guid noteGuid, CancellationToken cancellationToken);
    Task CreateNoteAsync(EFNote note, CancellationToken cancellationToken);
}
