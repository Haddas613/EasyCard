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
using Z.EntityFramework.Plus;

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

        public IQueryable<MasavFileRow> GetMasavFileRows()
        {
            return GetRowsInternal().AsNoTracking();
        }

        public IQueryable<MasavFile> GetMasavFiles()
        {
            return GetFilesInternal().AsNoTracking();
        }

        public async Task<MasavFile> GetMasavFile(long masavFileID)
        {
            var masavFile = GetFilesInternal().Where(d => d.MasavFileID == masavFileID).Future();
            var masavFileRows = GetRowsInternal().Where(d => d.MasavFileID == masavFileID).Future();

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

        public async Task<long> GenerateMasavFile(Guid? merchantID, Guid? terminalID, string institueName, int? sendingInstitute, string instituteNumber, DateTime? masavFileDate)
        {
            return await context.GenerateMasavFile(merchantID, terminalID, institueName, sendingInstitute, instituteNumber, masavFileDate);
        }

        public Task SetMasavFilePayed(long masavFileID, DateTime payedDate)
        {
            throw new NotImplementedException();
        }

        private IQueryable<MasavFileRow> GetRowsInternal()
        {
            if (user.IsAdmin())
            {
                return context.MasavFileRows;
            }
            else if (user.IsTerminal())
            {
                var terminalID = user.GetTerminalID()?.FirstOrDefault();
                return context.MasavFileRows.Where(t => t.MasavFile.TerminalID == terminalID);
            }
            else
            {
                var response = context.MasavFileRows.Where(t => t.MasavFile.MerchantID == user.GetMerchantID());
                var terminals = user.GetTerminalID();
                if (terminals?.Count() > 0)
                {
                    response = response.Where(d => terminals.Contains(d.MasavFile.TerminalID.GetValueOrDefault()));
                }

                return response;
            }
        }

        private IQueryable<MasavFile> GetFilesInternal()
        {
            if (user.IsAdmin())
            {
                return context.MasavFiles;
            }
            else if (user.IsTerminal())
            {
                var terminalID = user.GetTerminalID()?.FirstOrDefault();
                return context.MasavFiles.Where(t => t.TerminalID == terminalID);
            }
            else
            {
                var response = context.MasavFiles.Where(t => t.MerchantID == user.GetMerchantID());
                var terminals = user.GetTerminalID();
                if (terminals?.Count() > 0)
                {
                    response = response.Where(d => terminals.Contains(d.TerminalID.GetValueOrDefault()));
                }

                return response;
            }
        }
    }
}
