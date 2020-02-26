using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Extensions.Filtering
{
    public static class CommonFilteringExtensions
    {
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> src, FilterBase filter)
        {
            if (filter?.Skip != null) src = src.Skip(filter.Skip.Value);

            src = src.Take(filter?.Take ?? 100);

            return src;
        }
    }
}
