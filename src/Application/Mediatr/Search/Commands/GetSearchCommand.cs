using BanHub.Domain.Enums;
using MediatR;

namespace BanHub.Application.Mediatr.Search.Commands;

public class GetSearchCommand : IRequest<(ControllerEnums.ReturnState State, DTOs.WebView.SearchView.Search? Search)>
{
    public string Query { get; set; }
}
