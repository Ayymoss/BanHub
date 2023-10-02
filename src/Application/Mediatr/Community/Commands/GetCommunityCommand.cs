using MediatR;

namespace BanHub.Application.Mediatr.Community.Commands;

public class GetCommunityCommand : IRequest<DTOs.WebView.CommunityProfileView.Community?>
{
    public Guid CommunityGuid { get; set; }
}
