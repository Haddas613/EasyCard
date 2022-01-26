using Nayax.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Nayax.Converters;
using Nayax.Models;
using Shared.Integration.Models;

namespace Nayax.Converters
{
    public static class EMVDealHelper
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
            inputObj.mutav = req.SapakMutavNo.GetNayaxSapakMutav();

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
                AuthNum = resultPhase2Body.issuerAuthNum,
                Solek = (SolekEnum)resultPhase2Body.solek,
                ShvaShovarNumber = resultPhase2Body.sysTraceNumber,
                CreditCardVendor = (CardVendorEnum)resultPhase2Body.manpik,
                Success = ((PhaseResultEnum)Convert.ToInt32(resultPhase2Body.statusCode)).IsSuccessful(),
                PinPadTransactionID = resultPhase2Body.vuid,
                CardExpiration = new Shared.Helpers.CardExpiration
                {
                    Month = int.Parse(resultPhase2Body.expDate.Substring(2, 2)),
                    Year = int.Parse(resultPhase2Body.expDate.Substring(0, 2))
                },
                ResultCode = Convert.ToInt32(resultPhase2Body.statusCode)
            };
        }

        public static string GetFilNSeq(Shared.Integration.Models.Processor.ShvaTransactionDetails lastDeal)
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


        public static string GetFilNSeq(string ShvaShovarNumber,DateTime? TransmissionDate )
        {
            int fileNo = -1;
            int seqNo = -1;
            bool firstDeal = String.IsNullOrEmpty(ShvaShovarNumber);
            if (firstDeal)
            {
                fileNo = seqNo = 1;

            }
            else
            {
                string dealnumber = ShvaShovarNumber;

                int.TryParse(dealnumber.Substring(0, 2), out fileNo);

                int.TryParse(dealnumber.Substring(5, 3), out seqNo);
                bool lastDealWasTransmit = TransmissionDate != null;
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
