using Merchants.Business.Entities.Billing;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public interface ICurrencyRateService : IServiceBase<CurrencyRate, long>
    {
        IQueryable<CurrencyRate> GetRates();

        Task<CurrencyRateTuple> GetLatestRates(DateTime? date = null);

        Task CreateOrUpdate(CurrencyRate currencyRate);
    }
}
