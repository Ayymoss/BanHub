using MediatR;

namespace BanHub.WebCore.Shared.Commands.Search;

public class GetSearchCommand : IRequest<IEnumerable<Models.SearchView.Search>>
{
    public string Query { get; set; }
}
