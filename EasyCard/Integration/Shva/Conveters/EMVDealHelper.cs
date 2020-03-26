using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Integration.Models;
using Shva.Configuration;
using Shva.Models;
using ShvaEMV;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Conveters
{
    internal static class EMVDealHelper
    {
        public static AshStartRequestBody GetAshStartRequestBody(this ShvaTerminalSettings shvaParameters)
        {
            var ashStartReq = new AshStartRequestBody();

            ashStartReq.UserName = shvaParameters.UserName;
            ashStartReq.Password = shvaParameters.Password;
            ashStartReq.MerchantNumber = shvaParameters.MerchantNumber;

            return ashStartReq;
        }

        public static AshEndRequestBody GetAshEndRequestBody(this ShvaTerminalSettings shvaParameters)
        {
            var ashEndReq = new AshEndRequestBody();

            ashEndReq.UserName = shvaParameters.UserName;
            ashEndReq.Password = shvaParameters.Password;
            ashEndReq.MerchantNumber = shvaParameters.MerchantNumber;

            return ashEndReq;
        }

        public static AshAuthRequestBody GetAshAuthRequestBody(this AshStartResponseBody ashStartResultBody, ShvaTerminalSettings shvaParameters)
        {
            var ashAuthReq = new AshAuthRequestBody();

            ashAuthReq.pinpad = ashStartResultBody.pinpad;
            ashAuthReq.globalObj = ashStartResultBody.globalObj;

            ashAuthReq.MerchantNumber = shvaParameters.MerchantNumber;
            ashAuthReq.UserName = shvaParameters.UserName;
            ashAuthReq.Password = shvaParameters.Password;

            return ashAuthReq;
        }

        public static ShvaCreateTransactionResponse GetProcessorTransactionResponse(this AshEndResponseBody resultAshEndBody)
        {
            var resCode = (AshEndResultEnum)resultAshEndBody.AshEndResult;

            if (resCode.IsSuccessful())
            {
                var shvaDetails = new ShvaCreatedTransactionDetails
                {
                    ShvaShovarNumber = resultAshEndBody.globalObj?.receiptObj?.voucherNumber?.valueTag,
                    ShvaDealID = resultAshEndBody.globalObj?.outputObj?.tranRecord?.valueTag,
                    AuthSolekNum = resultAshEndBody.globalObj?.outputObj?.authSolekNo?.valueTag,
                    AuthNum = resultAshEndBody.globalObj?.outputObj?.authManpikNo?.valueTag,
                };

                return new ShvaCreateTransactionResponse() { ProcessorTransactionDetails = shvaDetails };
            }
            else
            {
                var errorCode = resCode.GetErrorCode();
                string errorCodeStr = resultAshEndBody.AshEndResult.ToString();
                return new ShvaCreateTransactionResponse(Messages.Failed, errorCode, errorCodeStr);
            }
        }

        public static clsInput GetInitInitObjRequest(this ProcessorCreateTransactionRequest req)
        {
            clsInput inputObj = new clsInput();
            InitDealResultModel initialDealData = req.InitDealResultData as InitDealResultModel;
            int parameterJValue = (int)req.JDealType.GetParamJ5();
            var transactionType = req.TransactionType.GetShvaTransactionType();
            var cardPresence = req.CardPresence.GetShvaCardPresence();
            var shvaExpDate = req.CreditCardToken.CardExpiration.GetShvaExpDate();
            var creditTerms = req.TransactionType.GetShvaCreditTerms();
            var currency = req.Currency.GetShvaCurrency();

            if (transactionType == ShvaTransactionTypeEnum.InitialDeal)
            {
                inputObj.panEntryMode = cardPresence.GetShvaCardPresenceStr();

                inputObj.parameterJ = parameterJValue.ToString();
                inputObj.creditTerms = creditTerms.GetShvaCreditTermsStr();
                inputObj.tranType = transactionType.GetShvaTransactionTypeStr();
                inputObj.currency = currency.GetShvaCurrencyStr();

                if (initialDealData == null)
                {
                    //CVV
                    inputObj.cvv2 = req.CreditCardToken.Cvv;
                    inputObj.clientInputPan = req.CreditCardToken.CardNumber;
                    inputObj.expirationDate = shvaExpDate;

                    if (!string.IsNullOrWhiteSpace(req.CreditCardToken.CardOwnerNationalId))
                    {
                        inputObj.id = req.CreditCardToken.CardOwnerNationalId;
                    }

                    // static values for initial deal
                    inputObj.amount = "1";
                    inputObj.stndOrdrFreq = "4";
                    inputObj.stndOrdrTotalNo = "999";
                    inputObj.stndOrdrNo = "0";
                }
                else //this case is billing deal after initialization
                {
                    //implement billing deal after initialization
                    inputObj.amount = req.TransactionAmount.ToShvaDecimalStr();

                    inputObj.originalUid = initialDealData.OriginalUid;
                    inputObj.originalTranDate = initialDealData.OriginalTranTime;
                    inputObj.originalTranTime = initialDealData.OriginalTranTime;

                    //maximum of billing deals with the same details
                    inputObj.stndOrdrTotalNo = "999";

                    inputObj.originalAmount = initialDealData.Amount.ToString(); //return from initialzation deal
                    inputObj.stndOrdrNo = req.CurrentInstallment.ToString(); // counter of billing deal with the same details

                    if (!string.IsNullOrEmpty(initialDealData.OriginalAuthSolekNum))
                    {
                        inputObj.originalAuthSolekNum = initialDealData.OriginalAuthSolekNum;
                        inputObj.originalAuthorizationCodeSolek = "7"; // TODO: constant ot enum
                    }

                    if (!string.IsNullOrEmpty(initialDealData.OriginalAuthNum))
                    {
                        inputObj.originalAuthNum = initialDealData.OriginalAuthNum;
                        inputObj.originalAuthorizationCodeManpik = "7"; // TODO: constant ot enum
                    }
                }
            }
            else
            {
                inputObj.amount = req.TransactionAmount.ToShvaDecimalStr();
                inputObj.cvv2 = req.CreditCardToken.Cvv;
                inputObj.clientInputPan = req.CreditCardToken.CardNumber;
                inputObj.expirationDate = shvaExpDate;

                if (!string.IsNullOrWhiteSpace(req.CreditCardToken.CardOwnerNationalId))
                {
                    inputObj.id = req.CreditCardToken.CardOwnerNationalId;
                }

                if (!string.IsNullOrWhiteSpace(req.AuthNum))
                {
                    inputObj.authorizationNo = req.AuthNum;
                    inputObj.authorizationCodeManpik = "5";
                }

                if (creditTerms == ShvaCreditTermsEnum.Installment && req.NumberOfPayments > 1)
                {
                    inputObj.noPayments = req.NumberOfPayments.ToString();
                    inputObj.firstPayment = req.InitialPaymentAmount.ToShvaDecimalStr();
                    inputObj.notFirstPayment = req.InstallmentPaymentAmount.ToShvaDecimalStr(); // amount in other installments
                }

                if (creditTerms == ShvaCreditTermsEnum.Credit && req.NumberOfPayments > 1)
                {
                    inputObj.noPayments = req.NumberOfPayments.ToString();
                }
            }

            return inputObj;
        }
    }
}
