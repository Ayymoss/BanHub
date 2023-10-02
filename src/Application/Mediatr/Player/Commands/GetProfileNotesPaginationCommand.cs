using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Player.Commands;

public class GetProfileNotesPaginationCommand : Pagination, IRequest<PaginationContext<DTOs.WebView.PlayerProfileView.Note>>
{
    public bool Privileged { get; set; }
    public string Identity { get; set; }
}
