using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.PaymentRequests.Enums;
using Transactions.Shared.Enums;

namespace Transactions.Api.Extensions
{
    public static class PaymentRequestsHelper
    {
        public static PayReqQuickStatusFilterTypeEnum GetQuickStatus(this PaymentRequestStatusEnum @enum)
        {
            if ((int)@enum >= 1 && (int)@enum <= 40)
            {
                return PayReqQuickStatusFilterTypeEnum.Pending;
            }

            if (@enum == PaymentRequestStatusEnum.Payed)
            {
                return PayReqQuickStatusFilterTypeEnum.Completed;
            }

            if (@enum == PaymentRequestStatusEnum.Canceled)
            {
                return PayReqQuickStatusFilterTypeEnum.Canceled;
            }

            if (@enum == PaymentRequestStatusEnum.Rejected)
            {
                return PayReqQuickStatusFilterTypeEnum.Overdue;
            }

            if ((int)@enum < 0)
            {
                return PayReqQuickStatusFilterTypeEnum.Failed;
            }

            return PayReqQuickStatusFilterTypeEnum.Pending;
        }
    }
}
