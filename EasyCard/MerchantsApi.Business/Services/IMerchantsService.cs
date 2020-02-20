using MerchantsApi.Business.Entities;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantsApi.Business.Services
{
    public interface IMerchantsService : IServiceBase
    {
        IQueryable<Merchant> GetMerchants();

        void AddMerchant(Merchant merchant);
    }
}
