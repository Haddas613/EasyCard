using MerchantsApi.Business.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantsApi.Business.Services
{
    public class MerchantsService : IMerchantsService
    {
        public IQueryable<Merchant> GetMerchants()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResponse> SaveChanges(IDbContextTransaction dbTransaction = null)
        {
            throw new NotImplementedException();
        }
    }
}
