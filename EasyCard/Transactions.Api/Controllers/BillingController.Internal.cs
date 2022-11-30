using Microsoft.EntityFrameworkCore.Storage;
using Shared.Api.Models;
using Shared.Integration.Models;
using System;
using System.Threading.Tasks;
using Transactions.Shared;
using Transactions.Shared.Models;
using Merchants.Business.Entities.Terminal;

namespace Transactions.Api.Controllers
{
    public partial class BillingController
    {
        private async Task<OperationResponse> CheckDuplicateBillingDeal(Terminal terminalDetails, BillingDealCompare billingDealCompare, PaymentTypeEnum paymentType, IDbContextTransaction dbContextTransaction)
        {
            bool hasFeature = terminalDetails.EnabledFeatures.Contains(Merchants.Shared.Enums.FeatureEnum.PreventDoubleTansactions);

            if (!hasFeature)
            {
                return null;
            }
            DateTime? threshold = hasFeature ? DateTime.UtcNow.AddMinutes(-(terminalDetails.Settings.MinutesToWaitBetDuplicateTransactions ?? 1)) : (DateTime?)null;

            var res = await billingDealService.CheckDuplicateBillingDeal(billingDealCompare, threshold, paymentType, dbContextTransaction);

            if (res)
            {
                return new OperationResponse(Messages.DuplicateBillingDealIsDetected, (Guid?)null, GetCorrelationID(), "DuplicateBillingDeal", Messages.DuplicateBillingDealIsDetected);
            }
            else
            {
                return null;
            }
        }
    }
}