using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Integration.Models;
using Shva.Models;
using ShvaEMV;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Conveters
{
    internal static class EMVDealHelper
    {
        public static ChangePasswordRequestBody GetChangePasswordRequestBody(this ShvaTerminalSettings shvaParameters, string newPassword)
        {
            var changePasswordReq = new ChangePasswordRequestBody();

            changePasswordReq.UserName = shvaParameters.UserName;
            changePasswordReq.Password = shvaParameters.Password;
            changePasswordReq.MerchantNumber = shvaParameters.MerchantNumber;
            changePasswordReq.NewPassword = newPassword;
            return changePasswordReq;
        }

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
            return new ShvaCreateTransactionResponse()
            {
                ShvaShovarNumber = resultAshEndBody.globalObj?.receiptObj?.voucherNumber?.valueTag,
                ResultCode = resultAshEndBody.AshEndResult,
                ShvaTranRecord = resultAshEndBody.globalObj?.outputObj?.tranRecord?.valueTag,
                ShvaDealID = resultAshEndBody.globalObj?.outputObj?.uid?.valueTag,
                AuthSolekNum = resultAshEndBody.globalObj?.outputObj?.authSolekNo?.valueTag,
                AuthNum = resultAshEndBody.globalObj?.outputObj?.authManpikNo?.valueTag,
                Brand = resultAshEndBody.globalObj?.outputObj?.brand?.valueTag,
                Solek = (SolekEnum)Convert.ToInt32(resultAshEndBody.globalObj?.outputObj?.solek?.valueTag),
                CreditCardVendor = (CardVendorEnum)Convert.ToInt32(resultAshEndBody.globalObj?.outputObj?.manpik?.valueTag), // TODO
                ShvaTransactionDate = resultAshEndBody.globalObj?.outputObj?.dateTime?.valueTag?.GetDateFromShvaDateTime(),
                TranRecord = resultAshEndBody.globalObj?.outputObj?.tranRecord?.valueTag,
                EmvSoftVersion = resultAshEndBody.globalObj?.outputObj?.appVersion?.valueTag,
                CompRetailerNum = resultAshEndBody.globalObj?.outputObj?.compRetailerNum?.valueTag
            };
        }

        public static ShvaCreateTransactionResponse GetOKNumberRequiredProcessorTransactionResponse(this AshEndResponseBody resultAshEndBody)
        {
            string telToGetAuthNum = resultAshEndBody.globalObj?.outputObj?.telNoCom?.valueTag;
            string compRetailerNum = resultAshEndBody.globalObj?.outputObj?.compRetailerNum?.valueTag;
            return new ShvaCreateTransactionResponse(resultAshEndBody.globalObj.outputObj.ashStatusDes.valueTag, resultAshEndBody.globalObj.outputObj.ashStatus.valueTag, telToGetAuthNum, compRetailerNum, resultAshEndBody.AshEndResult);
        }

        public static ShvaCreateTransactionResponse GetOKNumberRequiredProcessorTransactionResponse(this AshAuthResponseBody resultAshAuthBody)
        {
            string telToGetAuthNum = resultAshAuthBody.globalObj?.outputObj?.telNoCom?.valueTag;
            string compRetailerNum = resultAshAuthBody.globalObj?.outputObj?.compRetailerNum?.valueTag;
            return new ShvaCreateTransactionResponse(resultAshAuthBody.globalObj.outputObj.ashStatusDes?.valueTag, resultAshAuthBody.globalObj.outputObj.ashStatus?.valueTag, telToGetAuthNum, compRetailerNum, resultAshAuthBody.AshAuthResult);
        }

        public static ShvaCreateTransactionResponse GetProcessorTransactionResponse(this AshStartResponseBody resultAshStartBody)
        {
            return new ShvaCreateTransactionResponse()
            {
                ShvaShovarNumber = resultAshStartBody.globalObj?.receiptObj?.voucherNumber?.valueTag,
                ResultCode = resultAshStartBody.AshStartResult,
                ShvaTranRecord = resultAshStartBody.globalObj?.outputObj?.tranRecord?.valueTag,
                ShvaDealID = resultAshStartBody.globalObj?.outputObj?.uid?.valueTag,
                AuthSolekNum = resultAshStartBody.globalObj?.outputObj?.authSolekNo?.valueTag,
                AuthNum = resultAshStartBody.globalObj?.outputObj?.authManpikNo?.valueTag,
                Brand = resultAshStartBody.globalObj?.outputObj?.brand?.valueTag,
                Solek = (SolekEnum)Convert.ToInt32(resultAshStartBody.globalObj?.outputObj?.solek?.valueTag),
                CreditCardVendor = (CardVendorEnum)Convert.ToInt32(resultAshStartBody.globalObj?.outputObj?.manpik?.valueTag), // TODO
                ShvaTransactionDate = resultAshStartBody.globalObj?.outputObj?.dateTime?.valueTag?.GetDateFromShvaDateTime()
            };
        }

        public static ProcessorChangePasswordResponse GetProcessorChangePasswordResponse(this ChangePasswordResponseBody changePasswordBody)
        {
            return new ProcessorChangePasswordResponse()
            {
                Success = changePasswordBody.ChangePasswordResult == (int)ChangePasswordResultEnum.Success
            };
        }

        public static clsInput GetInitInitObjRequest(this ProcessorCreateTransactionRequest req)
        {
            clsInput inputObj = new clsInput();
            var cardPresence = req.CardPresence.GetShvaCardPresence();

            if (req.ThreeDSecure != null)
            {
                inputObj.xid = req.ThreeDSecure.Xid;
                inputObj.eci = req.ThreeDSecure.Eci.GetShvaEciBy3dsEci();
                inputObj.cavvUcaf = req.ThreeDSecure.AuthenticationValue;
                inputObj.addendum2 = req.ThreeDSecure.Eci.GetShvaAddendum2By3dsEci(req.ThreeDSecure.DsTransID);
                cardPresence = ShvaCardPresenceEnum.InternetDeal;
            }

            InitDealResultModel initialDealData = req.InitialDeal as InitDealResultModel;
            int parameterJValue = (int)req.JDealType.GetParamJ5();
            var transactionType = req.SpecialTransactionType.GetShvaTransactionType();
            var shvaExpDate = req.CreditCardToken.CardExpiration?.GetShvaExpDate();
            var creditTerms = req.TransactionType.GetShvaCreditTerms();
            var currency = req.Currency.GetShvaCurrency();
            var oKNumber = req.OKNumber;
            inputObj.sapakMutavNo = req.SapakMutavNo;
            inputObj.panEntryMode = cardPresence.GetShvaCardPresenceStr();

            inputObj.parameterJ = parameterJValue.ToString();
            inputObj.creditTerms = creditTerms.GetShvaCreditTermsStr();
            inputObj.tranType = transactionType.GetShvaTransactionTypeStr();
            inputObj.currency = currency.GetShvaCurrencyStr();
            if (!string.IsNullOrEmpty(oKNumber))
            {
                inputObj.authorizationNo = oKNumber;
                inputObj.authorizationCodeManpik = "5";
            }

            if (transactionType == ShvaTransactionTypeEnum.InitialDeal)
            {
                if (initialDealData == null)
                {
                    //CVV
                    inputObj.cvv2 = req.CreditCardToken.Cvv;
                    inputObj.clientInputPan = cardPresence == ShvaCardPresenceEnum.Magnetic ? req.CreditCardToken.CardReaderInput : req.CreditCardToken.CardNumber;
                    inputObj.expirationDate = shvaExpDate;

                    // TODO: national ID
                    if (!string.IsNullOrWhiteSpace(req.CreditCardToken.CardOwnerNationalID))
                    {
                        inputObj.id = req.CreditCardToken.CardOwnerNationalID;
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
                    inputObj.originalTranDate = initialDealData.OriginalTranDateTime.GetShvaDateStr();
                    inputObj.originalTranTime = initialDealData.OriginalTranDateTime.GetShvaTimeStr();

                    //maximum of billing deals with the same details
                    inputObj.stndOrdrTotalNo = "999";

                    inputObj.originalAmount = "1"; // initialDealData.Amount.ToString(); //return from initialzation deal
                    inputObj.stndOrdrNo = 1.ToString(); // req.CurrentDeal.ToString(); // counter of billing deal with the same details

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
                if (initialDealData != null)
                {
                    //implement billing deal after initialization
                    inputObj.amount = req.TransactionAmount.ToShvaDecimalStr();

                    inputObj.originalUid = initialDealData.OriginalUid;
                    inputObj.originalTranDate = initialDealData.OriginalTranDateTime.GetShvaDateStr();
                    inputObj.originalTranTime = initialDealData.OriginalTranDateTime.GetShvaTimeStr();

                    //maximum of billing deals with the same details
                    inputObj.stndOrdrTotalNo = "999";

                    inputObj.originalAmount = "1"; // initialDealData.Amount.ToString(); //return from initialzation deal
                    inputObj.stndOrdrNo = 1.ToString(); // req.CurrentDeal.ToString(); // counter of billing deal with the same details

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

                inputObj.amount = req.TransactionAmount.ToShvaDecimalStr();
                inputObj.cvv2 = req.CreditCardToken.Cvv;
                inputObj.clientInputPan = cardPresence == ShvaCardPresenceEnum.Magnetic ? req.CreditCardToken.CardReaderInput : req.CreditCardToken.CardNumber;
                inputObj.expirationDate = shvaExpDate;

                // TODO: national ID
                if (!string.IsNullOrWhiteSpace(req.CreditCardToken.CardOwnerNationalID))
                {
                    inputObj.id = req.CreditCardToken.CardOwnerNationalID;
                }

                if (!string.IsNullOrWhiteSpace(req.CreditCardToken.AuthNum))
                {
                    inputObj.authorizationNo = req.CreditCardToken.AuthNum;
                    inputObj.authorizationCodeManpik = "5";
                }

                if (creditTerms == ShvaCreditTermsEnum.Installment && req.NumberOfPayments > 1)
                {
                    inputObj.noPayments = (req.NumberOfPayments - 1).ToString();
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
