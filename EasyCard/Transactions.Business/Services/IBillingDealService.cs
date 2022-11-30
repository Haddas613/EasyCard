using Microsoft.EntityFrameworkCore.Storage;
using Shared.Api.Models;
using Shared.Business;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;
using Transactions.Shared.Models;

namespace Transactions.Business.Services
{
    public interface IBillingDealService : IServiceBase<BillingDeal, Guid>
    {
        IQueryable<BillingDeal> GetBillingDeals();

        Task<BillingDeal> GetBillingDeal(Guid billingDeal);

        IQueryable<BillingDealHistory> GetBillingDealHistory(Guid billingDealID);

        Task UpdateEntityWithHistory(BillingDeal entity, string message, BillingDealOperationCodesEnum operationCode, IDbContextTransaction dbTransaction = null);

        Task<bool> CheckDuplicateBillingDeal(BillingDealCompare billingDealCompare, DateTime? threshold, PaymentTypeEnum paymentType, IDbContextTransaction dbContextTransaction);
    }
}
