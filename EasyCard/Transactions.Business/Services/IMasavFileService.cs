using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public interface IMasavFileService
    {
        IQueryable<MasavFile> GetMasavFiles();

        IQueryable<MasavFileRow> GetMasavFileRows();

        Task<MasavFileRow> GetMasavFileRow(Expression<Func<MasavFileRow, bool>> predicate);

        Task UpdateMasavFileRow(MasavFileRow data);

        Task CreateMasavFileRow(MasavFileRow data);
    }
}
