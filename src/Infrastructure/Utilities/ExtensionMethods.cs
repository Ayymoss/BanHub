using System.Linq.Expressions;
using BanHub.Domain.Enums;
using BanHub.Domain.ValueObjects.Services;

namespace BanHub.Infrastructure.Utilities;

public static class ExtensionMethods
{
    public static IQueryable<TDomain> ApplySort<TDomain>(this IQueryable<TDomain> query, SortDescriptor sort,
        Expression<Func<TDomain, object>> property)
    {
        return sort.SortOrder is SortDirection.Ascending
            ? query.OrderBy(property)
            : query.OrderByDescending(property);
    }
}
