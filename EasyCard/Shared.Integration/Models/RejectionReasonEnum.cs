using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Shared.Integration.Models
{
    public enum RejectionReasonEnum : short
    {
        // TODO: add required codes
        [EnumMember(Value = "unknown")]
        Unknown = 0,

        [EnumMember(Value = "creditCardIsMerchantsCard")]
        CreditCardIsMerchantsCard = 1,

        [EnumMember(Value = "nationalIdIsMerchantsId")]
        NationalIdIsMerchantsId = 2,

        [EnumMember(Value = "singleTransactionAmountExceeded")]
        SingleTransactionAmountExceeded = 3,

        [EnumMember(Value = "dailyAmountExceeded")]
        DailyAmountExceeded = 4,

        [EnumMember(Value = "creditCardDailyUsageExceeded")]
        CreditCardDailyUsageExceeded = 5,

        [EnumMember(Value = "refundNotMatchRegularAmount")]
        RefundNotMatchRegularAmount = 6,

        [EnumMember(Value = "refundExceededCollateral")]
        RefundExceededCollateral = 7,

        [EnumMember(Value = "cardOwnerNationalIdRequired")]
        CardOwnerNationalIdRequired = 8,

        [EnumMember(Value = "authCodeRequired")]
        AuthorizationCodeRequired = 9,
    }
}
