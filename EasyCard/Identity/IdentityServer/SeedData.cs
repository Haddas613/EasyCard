using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using IdentityServer.Data;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Helpers.Security;

namespace IdentityServer
{
    public class SeedData
    {
        public static void EnsureSeedData(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    // context.Database.Migrate();

                    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var alice = userMgr.FindByNameAsync("ecng-admin@e-c.co.il").Result;
                    if (alice == null)
                    {
                        alice = new ApplicationUser
                        {
                            UserName = "ecng-admin@e-c.co.il",
                            Email = "ecng-admin@e-c.co.il"
                        };
                        var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        var aliceClaims = new Claim[]
                            {
                                new Claim(JwtClaimTypes.GivenName, "Admin"),
                                new Claim(JwtClaimTypes.FamilyName, "Temporary"),
                            };

                        result = userMgr.AddClaimsAsync(alice, aliceClaims).Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        userMgr.AddToRoleAsync(alice, Roles.BusinessAdministrator);
                        userMgr.AddToRoleAsync(alice, Roles.BillingAdministrator);
                    }
                    else
                    {
                    }
                }
            }
        }
    }
}
