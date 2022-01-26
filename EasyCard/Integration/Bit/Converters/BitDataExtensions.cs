using Bit.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Converters
{
    public static class BitDataExtensions
    {
        public static BitCreateTransactionResponse GetBitCreateTransactionResponse(this BitCreateResponse bitCreateResponse)
        {
            return new BitCreateTransactionResponse
            {
                RequestAmount = bitCreateResponse.RequestAmount,
                CurrencyTypeCode = bitCreateResponse.CurrencyTypeCode,
                DebitMethodCode = bitCreateResponse.DebitMethodCode,
                ExternalSystemReference = bitCreateResponse.ExternalSystemReference,
                RequestSubjectDescription = bitCreateResponse.RequestSubjectDescription,
                FranchisingId = bitCreateResponse.FranchisingId,
                ProviderNbr = bitCreateResponse.ProviderNbr,
                UrlReturnAddress = bitCreateResponse.UrlReturnAddress,
                ApplicationSchemeAndroid = bitCreateResponse.ApplicationSchemeAndroid,
                ApplicationSchemeIos = bitCreateResponse.ApplicationSchemeIos,
                LinkAddresss = bitCreateResponse.LinkAddresss,
                PaymentInitiationId = bitCreateResponse.PaymentInitiationId,
                TransactionSerialId = bitCreateResponse.TransactionSerialId,
                PaymentPageUrlAddress = bitCreateResponse.PaymentPageUrlAddress,
            };
        }
    }
}
