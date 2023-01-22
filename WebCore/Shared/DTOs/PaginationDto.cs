namespace GlobalInfraction.WebCore.Shared.DTOs;

public class PaginationDto
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? SortLabel { get; set; } 
    public int? SortDirection { get; set; }
    public string? SearchString { get; set; }
}

