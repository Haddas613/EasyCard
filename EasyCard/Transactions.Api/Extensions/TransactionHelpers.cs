using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions.Enums;
using Transactions.Shared.Enums;

namespace Transactions.Api.Extensions
{
    public static class TransactionHelpers
    {
        public static QuickStatusFilterTypeEnum GetQuickStatus(this TransactionStatusEnum @enum, JDealTypeEnum jDealType)
        {
            if (@enum == Shared.Enums.TransactionStatusEnum.CancelledByMerchant)
            {
                return QuickStatusFilterTypeEnum.Canceled;
            }

            if (@enum == Shared.Enums.TransactionStatusEnum.AwaitingForTransmission)
            {
                return QuickStatusFilterTypeEnum.AwaitingForTransmission;
            }

            if ((int)@enum > 0 && (int)@enum < 40)
            {
                return QuickStatusFilterTypeEnum.Pending;
            }

            if (@enum == Shared.Enums.TransactionStatusEnum.Completed)
            {
                return QuickStatusFilterTypeEnum.Completed;
            }

            if ((int)@enum < 0)
            {
                return QuickStatusFilterTypeEnum.Failed;
            }

            return QuickStatusFilterTypeEnum.Pending;
        }
    }
}
