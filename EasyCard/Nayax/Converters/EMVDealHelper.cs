using Nayax.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Nayax.Converters;
using Nayax.Models;
using Shared.Integration.Models;

namespace Nayax.Converters
{
    internal static class EMVDealHelper
    {
        public static AuthenticateRequestBody GetAuthRequestBody(NayaxGlobalSettings conf, string otp, string TerminalIDDevice)
        {
            var authReq = new AuthenticateRequestBody(otp, conf.ClientID, TerminalIDDevice, TerminalIDDevice);// new Phase1RequestBody(conf.ClientID, nayaxParameters.TerminalID,String.Format("{0}_{1}",ECterminalID, nayaxParameters.TerminalID));
            return authReq;
        }
        public static PairRequestBody GetPairRequestBody(NayaxGlobalSettings conf, string posName, string TerminalIDDevice)
        {
            var phase1Req = new PairRequestBody(posName, conf.ClientID, TerminalIDDevice, TerminalIDDevice);// new Phase1RequestBody(conf.ClientID, nayaxParameters.TerminalID,String.Format("{0}_{1}",ECterminalID, nayaxParameters.TerminalID));
            return phase1Req;
        }
        public static Phase1RequestBody GetPhase1RequestBody(this NayaxTerminalSettings nayaxParameters, NayaxGlobalSettings conf)
        {
            var phase1Req = new Phase1RequestBody(conf.ClientID, nayaxParameters.TerminalID, nayaxParameters.TerminalID);// new Phase1RequestBody(conf.ClientID, nayaxParameters.TerminalID,String.Format("{0}_{1}",ECterminalID, nayaxParameters.TerminalID));
            return phase1Req;
        }

        public static Phase2RequestBody GetPhase2RequestBody(this NayaxTerminalSettings nayaxParameters, NayaxGlobalSettings conf)
        {
            var phase2Req = new Phase2RequestBody(conf.ClientID, nayaxParameters.TerminalID, nayaxParameters.TerminalID/*todo add clientid_  before terminalid for posid*/);
            return phase2Req;
        }

        public static DoPeriodicRequestBody GetDoPeriodicRequest(this NayaxTerminalSettings nayaxParameters, NayaxGlobalSettings conf)
        {
            var doPeriodic = new DoPeriodicRequestBody(conf.ClientID, nayaxParameters.TerminalID, nayaxParameters.TerminalID);
            return doPeriodic;
        }

        /*
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
        
        public static NayaxCreateTransactionResponse GetProcessorTransactionResponse(this AshEndResponseBody resultAshEndBody)
        {
            return new ShvaCreateTransactionResponse()
            {
                ShvaShovarNumber = resultAshEndBody.globalObj?.receiptObj?.voucherNumber?.valueTag,

                ShvaTranRecord = resultAshEndBody.globalObj?.outputObj?.tranRecord?.valueTag,
                ShvaDealID = resultAshEndBody.globalObj?.outputObj?.uid?.valueTag,
                AuthSolekNum = resultAshEndBody.globalObj?.outputObj?.authSolekNo?.valueTag,
                AuthNum = resultAshEndBody.globalObj?.outputObj?.authManpikNo?.valueTag,

                Solek = (SolekEnum)Convert.ToInt32(resultAshEndBody.globalObj?.outputObj?.solek?.valueTag),
                CreditCardVendor = (CardVendorEnum)Convert.ToInt32(resultAshEndBody.globalObj?.outputObj?.manpik?.valueTag), // TODO
                ShvaTransactionDate = resultAshEndBody.globalObj?.outputObj?.dateTime?.valueTag?.GetDateFromShvaDateTime()
            };
        }

        public static ShvaCreateTransactionResponse GetProcessorTransactionResponse(this AshStartResponseBody resultAshStartBody)
        {
            return new ShvaCreateTransactionResponse()
            {
                ShvaShovarNumber = resultAshStartBody.globalObj?.receiptObj?.voucherNumber?.valueTag,

                ShvaTranRecord = resultAshStartBody.globalObj?.outputObj?.tranRecord?.valueTag,
                ShvaDealID = resultAshStartBody.globalObj?.outputObj?.uid?.valueTag,
                AuthSolekNum = resultAshStartBody.globalObj?.outputObj?.authSolekNo?.valueTag,
                AuthNum = resultAshStartBody.globalObj?.outputObj?.authManpikNo?.valueTag,

                Solek = (SolekEnum)Convert.ToInt32(resultAshStartBody.globalObj?.outputObj?.solek?.valueTag),
                CreditCardVendor = (CardVendorEnum)Convert.ToInt32(resultAshStartBody.globalObj?.outputObj?.manpik?.valueTag), // TODO
                ShvaTransactionDate = resultAshStartBody.globalObj?.outputObj?.dateTime?.valueTag?.GetDateFromShvaDateTime()
            };
        }
        */
        public static ObjectInPhase1RequestParams GetObjectInPhase1RequestParams(this ProcessorCreateTransactionRequest req)
        {
            ObjectInPhase1RequestParams inputObj = new ObjectInPhase1RequestParams();
            // InitDealResultModel initialDealData = req.InitialDeal as InitDealResultModel;
            var transactionType = req.SpecialTransactionType.GetNayaxTransactionType();
            var creditTerms = req.TransactionType.GetNayaxCreditTerms();
            var currency = req.Currency.GetNayaxCurrency();

            inputObj.creditTerms = creditTerms.GetNayaxCreditTerms();
            inputObj.tranType = transactionType.GetNayaxTransactionType();
            inputObj.currency = currency.GetNayaxCurrencyStr();

            inputObj.amount = req.TransactionAmount.ToNayaxDecimal();
            inputObj.vuid = req.PinPadTransactionID = GetPinPadTransactionID(req.PinPadProcessorSettings as NayaxTerminalSettings);
            inputObj.tranCode = 1;
            inputObj.sysTraceNumber = GetFilNSeq(req.LastDealShvaDetails);
            // TODO: national ID
            if (!string.IsNullOrWhiteSpace(req.CreditCardToken.CardOwnerNationalID))
            {
                inputObj.cardHolderID = req.CreditCardToken.CardOwnerNationalID;
            }

            if (creditTerms == NayaxCreditTermsEnum.Payments && req.NumberOfPayments > 1)
            {
                inputObj.payments = (req.NumberOfPayments - 1);
                inputObj.firstPaymentAmount = req.InitialPaymentAmount.ToNayaxDecimal();
                inputObj.otherPaymentAmount = req.InstallmentPaymentAmount.ToNayaxDecimal(); // amount in other installments
            }

            if (creditTerms == NayaxCreditTermsEnum.Credit && req.NumberOfPayments > 1)
            {
                inputObj.creditPayments = req.NumberOfPayments;
            }

            return inputObj;
        }

        public static ObjectInPhase2RequestParams GetObjectInPhase2RequestParams(this ProcessorCreateTransactionRequest req)
        {
            ObjectInPhase2RequestParams inputObj = new ObjectInPhase2RequestParams();
            // InitDealResultModel initialDealData = req.InitialDeal as InitDealResultModel;
            var transactionType = req.SpecialTransactionType.GetNayaxTransactionType();
            var creditTerms = req.TransactionType.GetNayaxCreditTerms();
            var currency = req.Currency.GetNayaxCurrency();

            inputObj.vuid = req.PinPadTransactionID;
            inputObj.creditTerms = creditTerms.GetNayaxCreditTerms();


            if (creditTerms == NayaxCreditTermsEnum.Payments && req.NumberOfPayments > 1)
            {
                inputObj.payments = (req.NumberOfPayments - 1);
                inputObj.firstPaymentAmount = req.InitialPaymentAmount.ToNayaxDecimal();
                inputObj.otherPaymentAmount = req.InstallmentPaymentAmount.ToNayaxDecimal(); // amount in other installments
            }

            if (creditTerms == NayaxCreditTermsEnum.Credit && req.NumberOfPayments > 1)
            {
                inputObj.creditPayments = req.NumberOfPayments;
            }

            return inputObj;
        }

        public static NayaxPreCreateTransactionResponse GetProcessorPreTransactionResponse(this Phase1ResponseBody resultPhase1Body, string PinPadTransactionID)
        {
            return new NayaxPreCreateTransactionResponse()
            {
                UID = resultPhase1Body.uid,
                CardNumber = resultPhase1Body.cardNumber,
                Success = resultPhase1Body.IsSuccessful(),
                ErrorMessage = resultPhase1Body.statusMessage,
                PinPadTransactionID = PinPadTransactionID
            };
        }

        public static NayaxCreateTransactionResponse GetProcessorTransactionResponse(this Phase2ResponseBody resultPhase2Body)
        {
            return new NayaxCreateTransactionResponse()
            {
                CardNumber = resultPhase2Body.cardNumber,
                ShvaDealID = resultPhase2Body.uid,//resultPhase2Body.sysTraceNumber,
                AuthNum = resultPhase2Body.manpik.ToString(),
                Solek = (SolekEnum)resultPhase2Body.solek,
                ShvaShovarNumber = resultPhase2Body.sysTraceNumber,
                CreditCardVendor = (CardVendorEnum)resultPhase2Body.manpik,
                Success = ((PhaseResultEnum)Convert.ToInt32(resultPhase2Body.statusCode)).IsSuccessful(),
                PinPadTransactionID = resultPhase2Body.vuid,
                CardExpiration = new Shared.Helpers.CardExpiration { Month =int.Parse(resultPhase2Body.expDate.Substring(2, 2)), Year = int.Parse(resultPhase2Body.expDate.Substring(0, 2)) }

                    /*
                    public string statusCode { get; set; }
            public string statusMessage { get; set; }
            public double amount { get; set; }
            public int mutag { get; set; }
            public int manpik { get; set; }
            public int solek { get; set; }
            public string cardNumber { get; set; }
            public int tranType { get; set; }
            public int posEntryMode { get; set; }
            public int minCreditPayments { get; set; }
            public int maxCreditPayments { get; set; }
            public int minCreditAmount { get; set; }
            public int creditTerms { get; set; }

            public string issuerAuthNum { get; set; }
            public string rrn { get; set; }

            public string acquirerMerchantID { get; set; }
            public string expDate { get; set; }
            //For Emv Desktop
            public int authCodeManpik { get; set; }
            public string cardName { get; set; }
            public string camutagNamerdName { get; set; }

            */

                    //ShvaTranRecord = resultAshEndBody.globalObj?.outputObj?.tranRecord?.valueTag,

                    //ShvaTransactionDate = resultAshEndBody.globalObj?.outputObj?.dateTime?.valueTag?.GetDateFromShvaDateTime()
                };
        }

        private static string GetFilNSeq(Shared.Integration.Models.Processor.ShvaTransactionDetails lastDeal)
        {
            int fileNo = -1;
            int seqNo = -1;
            bool firstDeal = lastDeal == null || String.IsNullOrEmpty(lastDeal.ShvaShovarNumber);
            if (firstDeal)
            {
                fileNo = seqNo = 1;
                
            }
            else
            {
                string dealnumber = lastDeal.ShvaShovarNumber;

                int.TryParse(dealnumber.Substring(0, 2), out fileNo);

                int.TryParse(dealnumber.Substring(5, 3), out seqNo);
                bool lastDealWasTransmit = lastDeal.TransmissionDate != null;
                if (lastDealWasTransmit)
                {
                    seqNo = 1;
                    fileNo++;
                }

                if (seqNo > 999)
                {
                    seqNo = 1;
                    fileNo++;
                }
                else
                {
                    seqNo++;
                }

                if (fileNo == 100)
                {
                    fileNo = seqNo = 1;
                }
            }
            
            return string.Format("{0};{1}", fileNo, seqNo);
        }

        private static string GetPinPadTransactionID(NayaxTerminalSettings settings)
        {
            return string.Format("{0}_{1}", settings?.TerminalID, Guid.NewGuid().ToString());
        }
    }
}
