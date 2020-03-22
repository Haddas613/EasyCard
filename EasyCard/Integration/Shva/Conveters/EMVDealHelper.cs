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

        public static ProcessorTransactionResponse GetProcessorTransactionResponse(this AshEndResponseBody resultAshEndBody)
        {
            var resCode = (AshEndResultEnum)resultAshEndBody.AshEndResult;

            if (resCode.IsSuccessful())
            {
                var shvaDetails = new ShvaCreatedTransactionDetails
                {
                    ShvaShovarNumber = resultAshEndBody.globalObj.receiptObj.voucherNumber.valueTag,
                    ShvaDealID = resultAshEndBody.globalObj.outputObj.tranRecord.valueTag,
                    AuthSolekNum = resultAshEndBody.globalObj.outputObj.authSolekNo.valueTag,
                    AuthNum = resultAshEndBody.globalObj.outputObj.authManpikNo.valueTag,
                };

                return new ProcessorTransactionResponse() { ProcessorTransactionDetails = shvaDetails };
            }
            else
            {
                var errorCode = resCode.GetErrorCode();
                string errorCodeStr = resultAshEndBody.AshEndResult.ToString();
                return new ProcessorTransactionResponse(Messages.Failed, errorCode,errorCodeStr);
            }
        }

        public static clsInput GetInitInitObjRequest(this ProcessorTransactionRequest req)
        {
            clsInput inputObj = new clsInput();

            int parameterJValue = (int)req.JDealType.GetParamJ5();
            var transactionType = req.TransactionType.GetShvaTransactionType();
            var cardPresence = req.CardPresence.GetShvaCardPresence();
            var shvaExpDate = req.CreditCardToken.CardExpiration.GetShvaExpDate();
            var creditTerms = req.TransactionType.GetShvaCreditTerms();
            var currency = req.Currency.GetShvaCurrency();

            // initialization deal. TODO: what is transaction type ?
            if (transactionType == ShvaTransactionTypeEnum.InitialDeal)
            {
                inputObj.panEntryMode = cardPresence.GetShvaCardPresenceStr();

                inputObj.parameterJ = parameterJValue.ToString();
                inputObj.creditTerms = creditTerms.GetShvaCreditTermsStr();
                inputObj.tranType = transactionType.GetShvaTransactionTypeStr();
                inputObj.currency = currency.GetShvaCurrencyStr();

                var initialTransaction = req.InitialTransaction as ShvaCreatedTransactionDetails;

                if (initialTransaction == null)
                {
                    //CVV
                    inputObj.cvv2 = req.CreditCardToken.Cvv;
                    inputObj.clientInputPan = req.CreditCardToken.CardNumber; // can be card number or track2 value for example 5100460000371892=21102010000024291000
                    inputObj.expirationDate = shvaExpDate;

                    if (!string.IsNullOrWhiteSpace(req.CreditCardToken))
                    {
                        inputObj.id = req.Id;
                    }

                    // TODO: describe this values
                    inputObj.amount = "1";
                    inputObj.stndOrdrFreq = "4";
                    inputObj.stndOrdrTotalNo = "999";
                    inputObj.stndOrdrNo = "0";
                }
                else
                {
                    //implement billing deal after initialization
                    inputObj.amount = req.TransactionAmount.ToShvaDecimalStr();

                    //inputObj.originalUid = req.InitialTransaction.OriginalUid; TODO: How to get this value ?
                    inputObj.originalTranDate = initialTransaction.TransactionDate?.ToString("yyyy-MM-dd"); // TODO: format ?
                    inputObj.originalTranTime = initialTransaction.TransactionDate?.ToString("HH:mm:ss"); // TODO: format, timezone ?

                    // TODO: describe this value
                    inputObj.stndOrdrTotalNo = "999";

                    //inputObj.originalAmount = req.InitialTransaction.TransactionAmount.ToString(); // TODO: what value it should be ?
                    inputObj.stndOrdrNo = req.CurrentInstallment.ToString(); // TODO: what value it should be ?

                    if (!string.IsNullOrEmpty(initialTransaction.AuthSolekNum))
                    {
                        inputObj.originalAuthSolekNum = initialTransaction.AuthSolekNum;
                        inputObj.originalAuthorizationCodeSolek = "7"; // TODO: constant ot enum
                    }

                    if (!string.IsNullOrEmpty(initialTransaction.AuthNum))
                    {
                        inputObj.originalAuthNum = initialTransaction.AuthNum;
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

                // TODO: how to get this ?
                //תעודת זהות
                //if (!string.IsNullOrWhiteSpace(req.Id))
                //{
                //    inputObj.id = req.Id;
                //}

                // TODO: how to get this ?
                //מספר אישור
                //if (!string.IsNullOrWhiteSpace(req.AuthNum))
                //{
                //    inputObj.authorizationNo = req.AuthNum;
                //    inputObj.authorizationCodeManpik = "5";
                //}

                //תשלומים
                // TODO: //Payments 8 OR 9
                if (creditTerms == ShvaCreditTermsEnum.Installment && req.NumberOfPayments > 1)
                {
                    inputObj.noPayments = req.NumberOfPayments.ToString();
                    inputObj.firstPayment = req.InitialPaymentAmount.ToShvaDecimalStr();
                    inputObj.notFirstPayment = req.InstallmentPaymentAmount.ToShvaDecimalStr(); // TODO: what amount is it ?
                }

                //קרדיט
                // TODO: what is Credit 5 OR 6
                // TODO: what NumberOfPayments should be ?
                if (creditTerms == ShvaCreditTermsEnum.Credit && req.NumberOfPayments > 1)
                {
                    inputObj.noPayments = req.NumberOfPayments.ToString();
                }
            }

            return inputObj;
        }
    }
}
