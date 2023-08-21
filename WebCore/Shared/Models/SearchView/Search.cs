namespace BanHub.WebCore.Shared.Models.SearchView;

public class Search
{
    public List<SearchPlayer> Players { get; set; }
    public List<SearchChat> Messages { get; set; }
}

public class SearchPlayer
{
    public string Username { get; set; }
    public string Identity { get; set; }
}

public class SearchChat
{
    public SearchPlayer Player { get; set; }
    public string Message { get; set; }
}
