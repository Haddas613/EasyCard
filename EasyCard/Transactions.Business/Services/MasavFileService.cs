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
using Z.EntityFramework.Plus;

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

        public async Task<MasavFile> GetMasavFile(long masavFileID)
        {
            var masavFile = context.MasavFiles.Where(d => d.MasavFileID == masavFileID).Future();
            var masavFileRows = context.MasavFileRows.Where(d => d.MasavFileID == masavFileID).Future();

            return (await masavFile.ToListAsync()).FirstOrDefault();
        }

        public async Task UpdateMasavFileRow(MasavFileRow data)
        {
            MasavFileRow exist = context.MasavFileRows.Find(data.GetID());

            context.Entry(exist).CurrentValues.SetValues(data);

            await context.SaveChangesAsync();
        }

        public async Task CreateMasavFileRow(MasavFileRow data)
        {
            context.MasavFileRows.Add(data);
            await context.SaveChangesAsync();
        }

        public Task CreateMasavFile(MasavFile data)
        {
            return CreateEntity(data);
        }

        public Task UpdateMasavFile(MasavFile data)
        {
            return UpdateEntity(data);
        }

        public async Task<long> GenerateMasavFile(Guid? terminalID, int? bank, int? bankBranch, string bankAccount, DateTime? masavFileDate)
        {
            return await context.GenerateMasavFile(terminalID, bank, bankBranch, bankAccount, masavFileDate);
        }
    }
}
