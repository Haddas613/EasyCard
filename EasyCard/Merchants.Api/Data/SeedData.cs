using Merchants.Api.Data.Seeds;
using Merchants.Business.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Data
{
    public class SeedData
    {
        public static void EnsureSeedData(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpContextAccessor();
            services.AddScoped<IHttpContextAccessorWrapper, HttpContextAccessorWrapper>();
            services.AddDbContext<MerchantsContext>(opts => opts.UseSqlServer(connectionString));

            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<MerchantsContext>();
                    context.Database.Migrate();

                    ExternalSystemsSeed.Seed(context);
                }
            }
        }
    }
}
