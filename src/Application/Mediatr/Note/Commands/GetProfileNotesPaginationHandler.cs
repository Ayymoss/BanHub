using BanHub.Application.Mediatr.Player.Commands;
using BanHub.Domain.Interfaces.Repositories.Pagination;
using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Note.Commands;

public class GetProfileNotesPaginationHandler(
        IResourceQueryHelper<GetProfileNotesPaginationCommand, DTOs.WebView.PlayerProfileView.Note> resourceQueryHelper)
    : IRequestHandler<GetProfileNotesPaginationCommand, PaginationContext<DTOs.WebView.PlayerProfileView.Note>>
{
    public async Task<PaginationContext<DTOs.WebView.PlayerProfileView.Note>> Handle(GetProfileNotesPaginationCommand request,
        CancellationToken cancellationToken)
    {
        var result = await resourceQueryHelper.QueryResourceAsync(request, cancellationToken);
        return result;
    }
}
