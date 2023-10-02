using BanHub.Domain.ValueObjects.Services;
using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class GetCommunitiesPaginationCommand : Pagination, IRequest<PaginationContext<DTOs.WebView.CommunitiesView.Community>>
{
    public bool Privileged { get; set; }
}
