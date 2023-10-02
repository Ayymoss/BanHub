using BanHub.Domain.Enums;

namespace BanHub.Domain.ValueObjects.Services;

public class SortDescriptor
{
    public string Property { get; set; }
    public SortDirection SortOrder { get; set; }
}
