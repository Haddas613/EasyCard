using Shared.Helpers.Models;
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
        public static TransactionTypeEnum GetTransactionTypeFromNayax(this CreditTermsEnum creditTerm)
        {
            return creditTerm switch
            {
                CreditTermsEnum.credit => TransactionTypeEnum.Credit,
                CreditTermsEnum.immediate => TransactionTypeEnum.Immediate,
                CreditTermsEnum.regular => TransactionTypeEnum.RegularDeal,
                CreditTermsEnum.installments => TransactionTypeEnum.Installments,
                _ => TransactionTypeEnum.RegularDeal,
            };
        }

        public static SolekEnum GetTransactionSolek(this IssuerAquirEnum issuerAquirEnum)
        {
            return issuerAquirEnum switch
            {
                IssuerAquirEnum.Cal => SolekEnum.VISA,
                IssuerAquirEnum.Isracard => SolekEnum.ISRACARD,
                IssuerAquirEnum.LeumiCard => SolekEnum.LEUMI_CARD,
                IssuerAquirEnum.Tourist => SolekEnum.UNKNOWN,
                IssuerAquirEnum.RFU07 => SolekEnum.OTHER,
                _ => SolekEnum.UNKNOWN,
            };
        }

        public static SpecialTransactionTypeEnum GetSpecialTransactionTypeFromNayax(this TranTypeEnum tranType)
        {
            return tranType switch
            {
                TranTypeEnum.refund => SpecialTransactionTypeEnum.Refund,
                TranTypeEnum.charge => SpecialTransactionTypeEnum.RegularDeal,
                _ => SpecialTransactionTypeEnum.RegularDeal,
            };
        }
    }
}
