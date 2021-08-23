using Microsoft.EntityFrameworkCore;
using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Data;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public class MasavFileService : ServiceBase<MasavFile, long>, IMasavFileService
    {
        private readonly TransactionsContext context;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly ClaimsPrincipal user;

        public MasavFileService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        private IQueryable<MasavFileRow> GetRowsInternal()
        {
            if (user.IsAdmin())
            {
                return context.MasavFileRows;
            }
            else if (user.IsTerminal())
            {
                return context.MasavFileRows.Where(t => t.MasavFile.TerminalID == user.GetTerminalID());
            }
            else
            {
                return context.MasavFileRows.Where(t => t.MasavFile.MerchantID == user.GetMerchantID());
            }
        }

        public IQueryable<MasavFileRow> GetMasavFileRows()
        {

            return GetRowsInternal().AsNoTracking();
        }

        private IQueryable<MasavFile> GetFilesInternal()
        {
            if (user.IsAdmin())
            {
                return context.MasavFiles;
            }
            else if (user.IsTerminal())
            {
                return context.MasavFiles.Where(t => t.TerminalID == user.GetTerminalID());
            }
            else
            {
                return context.MasavFiles.Where(t => t.MerchantID == user.GetMerchantID());
            }
        }

        public IQueryable<MasavFile> GetMasavFiles()
        {
            return GetFilesInternal().AsNoTracking();
        }

        public async Task<MasavFile> GetMasavFile(long masavFileID)
        {
            var masavFile = await GetFilesInternal().FirstOrDefaultAsync(d => d.MasavFileID == masavFileID);

            // TODO: rows (use future)

            return masavFile;
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
