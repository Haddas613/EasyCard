using Merchants.Business.Data;
using Merchants.Business.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace Merchants.Business.Services
{
    public class CurrencyRateService : ServiceBase<CurrencyRate, long>, ICurrencyRateService
    {
        private readonly MerchantsContext context;
        private readonly ClaimsPrincipal user;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        private readonly string currencyRateCacheTag = nameof(CurrencyRate);

        public CurrencyRateService(MerchantsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public IQueryable<CurrencyRate> GetRates() => context.CurrencyRates;

        public async Task<CurrencyRateTuple> GetLatestRates(DateTime? date = null)
        {
            var usdRate = await GetRates().OrderByDescending(d => d.Date)
                .Where(d => (date == null || d.Date <= date) && d.Currency == CurrencyEnum.USD)
                .DeferredFirstOrDefault()
                .FromCacheAsync(currencyRateCacheTag);

            var eurRate = await GetRates().OrderByDescending(d => d.Date)
                .Where(d => (date == null || d.Date <= date) && d.Currency == CurrencyEnum.EUR)
                .DeferredFirstOrDefault()
                .FromCacheAsync(currencyRateCacheTag);

            return new CurrencyRateTuple
            {
                EURRate = eurRate?.Rate,
                USDRate = usdRate?.Rate,
            };
        }

        public async Task CreateOrUpdate(CurrencyRate currencyRate)
        {
            var dbEntity = await context.CurrencyRates.FirstOrDefaultAsync(e => e.Currency == currencyRate.Currency && e.Date == currencyRate.Date);

            if (dbEntity != null) { return; }

            context.CurrencyRates.Add(currencyRate);
            await context.SaveChangesAsync();

            QueryCacheManager.ExpireTag(currencyRateCacheTag);
        }
    }
}
