using BanHub.Domain.ValueObjects.Services;

namespace BanHub.Domain.Interfaces.Repositories.Pagination;

public interface IResourceQueryHelper<in TQuery, TResult> where TQuery : ValueObjects.Services.Pagination where TResult : class
{
    Task<PaginationContext<TResult>> QueryResourceAsync(TQuery request, CancellationToken cancellationToken);
}
