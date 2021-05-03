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
        public static PayReqQuickStatusFilterTypeEnum GetQuickStatus(this PaymentRequestStatusEnum @enum, DateTime? dueDate)
        {
            if (@enum == PaymentRequestStatusEnum.Viewed)
            {
                return PayReqQuickStatusFilterTypeEnum.Viewed;
            }

            if ((int)@enum >= 1 && (int)@enum < 4)
            {
                return PayReqQuickStatusFilterTypeEnum.Pending;
            }

            if (@enum == PaymentRequestStatusEnum.Payed)
            {
                return PayReqQuickStatusFilterTypeEnum.Completed;
            }

            if (@enum == PaymentRequestStatusEnum.Canceled || @enum == PaymentRequestStatusEnum.Rejected)
            {
                return PayReqQuickStatusFilterTypeEnum.Canceled;
            }

            if (dueDate.HasValue && dueDate < DateTime.UtcNow)
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
