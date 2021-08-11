using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            throw new NotImplementedException();
        }

        public IQueryable<MasavFile> GetMasavFiles()
        {
            throw new NotImplementedException();
        }
    }
}
