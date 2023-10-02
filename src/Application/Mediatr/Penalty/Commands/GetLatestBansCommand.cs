using MediatR;

namespace BanHub.Application.Mediatr.Penalty.Commands;

public class GetLatestBansCommand : IRequest<IEnumerable<Application.DTOs.WebView.IndexView.Penalty>>
{
    public bool Privileged { get; set; }
}
