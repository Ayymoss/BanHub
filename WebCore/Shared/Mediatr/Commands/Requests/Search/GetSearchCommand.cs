using BanHubData.Enums;
using MediatR;

namespace BanHub.WebCore.Shared.Mediatr.Commands.Requests.Search;

public class GetSearchCommand : IRequest<(ControllerEnums.ReturnState State, Shared.Models.SearchView.Search? Search)>
{
    public string Query { get; set; }
}
