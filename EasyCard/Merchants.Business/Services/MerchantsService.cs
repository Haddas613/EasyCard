using Merchants.Business.Data;
using Merchants.Business.Entities.Merchant;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public class MerchantsService : ServiceBase<Merchant>, IMerchantsService
    {
        private readonly MerchantsContext context;
        public MerchantsService(MerchantsContext context): base(context)
        {
            this.context = context;
        }

        //public void AddMerchant(Merchant merchant)
        //{
        //    context.Add(merchant);
        //}

        public IQueryable<Merchant> GetMerchants()
        {
            return context.Merchants;
        }

        //public async Task<OperationResponse> SaveChanges(IDbContextTransaction dbTransaction = null)
        //{
        //    var result = new OperationResponse();

        //    try
        //    {
        //        await context.SaveChangesAsync();
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }

        //    return result;
        //}
    }
}
