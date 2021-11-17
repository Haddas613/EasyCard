using Merchants.Business.Data;
using Merchants.Business.Entities.System;
using Microsoft.EntityFrameworkCore;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace Merchants.Business.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly MerchantsContext context;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        private readonly string systemSettingsCacheTag = nameof(SystemSettings);

        public SystemSettingsService(MerchantsContext context, IHttpContextAccessorWrapper httpContextAccessor)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task UpdateSystemSettings(SystemSettings entity)
        {
            if (!httpContextAccessor.GetUser().IsAdmin())
            {
                throw new SecurityException("Method acces is not allowed");
            }

            using var transaction = context.Database.BeginTransaction();
            var exist = context.SystemSettings.Find(entity.SystemSettingsID);

            if (exist == null)
            {
                context.SystemSettings.Add(entity);
            }
            else
            {
                context.Entry(exist).CurrentValues.SetValues(entity);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            QueryCacheManager.ExpireTag(systemSettingsCacheTag);
        }

        public async Task<SystemSettings> GetSystemSettings()
        {
            return await context.SystemSettings.AsNoTracking().DeferredFirstOrDefault().FromCacheAsync(systemSettingsCacheTag);
        }
    }
}
