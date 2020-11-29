using Merchants.Business.Data;
using Merchants.Business.Entities.Billing;
using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Merchants.Business.Services
{
    public class ItemsService : ServiceBase<Item, Guid>, IItemsService
    {
        private readonly MerchantsContext context;
        private readonly ClaimsPrincipal user;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public ItemsService(MerchantsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            user = httpContextAccessor.GetUser();
        }

        public IQueryable<Item> GetItems()
        {
            if (user.IsAdmin())
            {
                return context.Items;
            }
            else
            {
                return context.Items.Where(t => t.MerchantID == user.GetMerchantID());
            }
        }
    }
}
