using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class GetCommunityProfilePenaltiesPaginationCommand : Pagination, IRequest<PaginationContext<DTOs.WebView.CommunityProfileView.Penalty>>
{
    public Guid Identity { get; set; }
    public bool Privileged { get; set; }
}
