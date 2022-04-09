using Microsoft.EntityFrameworkCore.Storage;
using Shared.Api.Models;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;

namespace Transactions.Business.Services
{
    public interface IBillingDealService : IServiceBase<BillingDeal, Guid>
    {
        IQueryable<BillingDeal> GetBillingDeals();

        IQueryable<BillingDeal> GetBillingDealsForUpdate();

        IQueryable<BillingDealHistory> GetBillingDealHistory(Guid billingDealID);

        Task AddCardTokenChangedHistory(BillingDeal billingDeal, Guid? newToken);

        Task UpdateEntityWithHistory(BillingDeal entity, string message, BillingDealOperationCodesEnum operationCode, IDbContextTransaction dbTransaction = null);
    }
}
