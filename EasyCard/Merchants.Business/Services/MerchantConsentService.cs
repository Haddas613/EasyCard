using Merchants.Business.Data;
using Merchants.Business.Entities.Merchant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public class MerchantConsentService : ServiceBase<MerchantConsent, Guid>, IMerchantConsentService
    {
        private readonly MerchantsContext context;
        private readonly ClaimsPrincipal user;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public MerchantConsentService(MerchantsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public override Task CreateEntity(MerchantConsent entity, IDbContextTransaction dbTransaction = null)
        {
            return base.CreateEntity(entity, dbTransaction);
        }
    }
}
