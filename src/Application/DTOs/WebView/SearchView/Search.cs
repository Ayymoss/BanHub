using BanHub.Domain.ValueObjects.Repository.Chat;

namespace BanHub.Application.DTOs.WebView.SearchView;

public class Search
{
    public List<SearchPlayer> Players { get; set; }
    public List<SearchChat> Messages { get; set; }
}
