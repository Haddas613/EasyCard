using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Masav;
using Transactions.Business.Entities;

namespace Transactions.Api.Extensions.Filtering
{
    public static class MasavFileFilteringExtensions
    {
        public static IQueryable<MasavFile> Filter(this IQueryable<MasavFile> src, MasavFileFilter filter)
        {
            return src;
        }

        public static IQueryable<MasavFileRow> Filter(this IQueryable<MasavFileRow> src, MasavFileRowFilter filter)
        {
            src = src.Where(r => r.MasavFileID == filter.MasavFileID);

            return src;
        }
    }
}
