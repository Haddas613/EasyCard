using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

        public IQueryable<MasavFile> GetMasavFiles()
        {
            return GetFilesInternal().AsNoTracking();
        }

        public async Task<MasavFile> GetMasavFile(long masavFileID)
        {
            var masavFile = GetFilesInternal().Where(d => d.MasavFileID == masavFileID).Future();
            var masavFileRows = context.MasavFileRows.Where(d => d.MasavFileID == masavFileID).Future();

            return (await masavFile.ToListAsync()).FirstOrDefault();
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

        public Task UpdateMasavFile(MasavFile data, IDbContextTransaction dbTransaction = null)
        {
            return UpdateEntity(data, dbTransaction);
        }

        public async Task<long> GenerateMasavFile(Guid? merchantID, Guid? terminalID, string institueName, int? sendingInstitute, string instituteNumber, DateTime? masavFileDate)
        {
            return await context.GenerateMasavFile(merchantID, terminalID, institueName, sendingInstitute, instituteNumber, masavFileDate);
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
                var terminals = user.GetTerminalID()?.Cast<Guid?>();
                if (terminals?.Count() > 0)
                {
                    response = response.Where(d => terminals.Contains(d.TerminalID));
                }

                return response;
            }
        }
    }
}
