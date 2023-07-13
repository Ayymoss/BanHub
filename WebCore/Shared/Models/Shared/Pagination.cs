namespace BanHub.WebCore.Shared.Models.Shared;

public class Pagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? SortLabel { get; set; } 
    public int SortDirection { get; set; }
    public string? SearchString { get; set; }
    public int TotalItems { get; set; }
}

