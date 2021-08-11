using Microsoft.EntityFrameworkCore;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Data;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public class MasavFileService : ServiceBase<MasavFile, long>, IMasavFileService
    {
        private readonly TransactionsContext context;

        public MasavFileService(TransactionsContext context)
            : base(context)
        {
            this.context = context;
        }

        public IQueryable<MasavFileRow> GetMasavFileRows()
        {
            return context.MasavFileRows.AsNoTracking();
        }

        public IQueryable<MasavFile> GetMasavFiles()
        {
            return context.MasavFiles.AsNoTracking();
        }

        public async Task<MasavFileRow> GetMasavFileRow(Expression<Func<MasavFileRow, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateMasavFileRow(MasavFileRow data)
        {
            throw new NotImplementedException();
        }

        public async Task CreateMasavFileRow(MasavFileRow data)
        {
            throw new NotImplementedException();
        }
    }
}
