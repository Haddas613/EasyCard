using Bit.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bit.Converters
{
    public static class BitDataExtensions
    {
        public static BitCreateTransactionResponse GetBitCreateTransactionResponse(this BitCaptureResponse bitCaptureResponse)
        {
            return new BitCreateTransactionResponse
            {
                RequestAmount = bitCaptureResponse.RequestAmount,
                CurrencyTypeCode = bitCaptureResponse.CurrencyTypeCode,
                ExternalSystemReference = bitCaptureResponse.ExternalSystemReference,
                PaymentInitiationId = bitCaptureResponse.PaymentInitiationId,
                SourceTransactionId = bitCaptureResponse.SourceTransactionId,
                IssuerTransactionId = bitCaptureResponse.IssuerTransactionId,
                IssuerAuthorizationNumber = bitCaptureResponse.IssuerAuthorizationNumber,
                SuffixPlasticCardNumber = bitCaptureResponse.SuffixPlasticCardNumber,
            };
        }
    }
}
