namespace BanHub.Domain.ValueObjects.Services;

public class PaginationContext<TContext> where TContext : class
{
    public IEnumerable<TContext> Data { get; set; }
    public int Count { get; set; }
}
