using Shared.Api.Models;
using Shared.Api.Models.Enums;
using ShvaEMV;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shva.Models
{
    internal static class EMVDealHelper
    {
        public static void InitInputObj(string expDate_YYMM, string transactionType, string currency, string code, string cardNum, string creditTerms, string amount, string cvv2, string authNum,
          string id, ParamJEnum paramJ, string numOfPayment, string firstAmount, string nonFirstAmount, InitDealResultModel initDealM, bool isNewInitDeal, ref clsInput inputObj)
        {

            int paramJInt = (int)paramJ;
            bool notvalidParamJ = paramJInt != (int)ParamJEnum.MakeDeal && paramJInt != (int)ParamJEnum.J5Deal && paramJInt != (int)ParamJEnum.Check;
            int parameterJValue = notvalidParamJ ? (int)ParamJEnum.MakeDeal : (int)paramJ;//J4 is default


            //initialization deal
            if ("11".Equals(transactionType))
            {
                inputObj.panEntryMode = code;
                inputObj.clientInputPan = cardNum;
                inputObj.expirationDate = expDate_YYMM;


                if (isNewInitDeal)
                {
                    //CVV
                    if (!String.IsNullOrWhiteSpace(cvv2))
                        inputObj.cvv2 = cvv2;

                    //תעודת זהות
                    if (!String.IsNullOrWhiteSpace(id))
                        inputObj.id = id;

                    inputObj.parameterJ = parameterJValue.ToString();
                    inputObj.creditTerms = creditTerms;
                    inputObj.tranType = transactionType;

                    inputObj.amount = "1";
                    inputObj.stndOrdrFreq = "4";
                    inputObj.stndOrdrTotalNo = "999";
                    inputObj.stndOrdrNo = "0";
                }
                else if (initDealM != null)
                {
                    //חיוב עסקת הוראת קבע אחרי אתחול
                    inputObj.creditTerms = creditTerms;
                    inputObj.tranType = transactionType;
                    inputObj.amount = Math.Round(Convert.ToDecimal(amount) / 100, 2).ToString("00.00").Replace(".", String.Empty);

                    inputObj.originalUid = initDealM.OriginalUid;
                    inputObj.originalTranDate = initDealM.OriginalTranDate;
                    inputObj.originalTranTime = initDealM.OriginalTranTime;
                    inputObj.stndOrdrTotalNo = "999";
                    inputObj.originalAmount = initDealM.Amount;
                    inputObj.stndOrdrNo = initDealM.DealsCounter.ToString();
                    if (!string.IsNullOrEmpty(initDealM.OriginalAuthSolekNum))
                    {
                        inputObj.originalAuthSolekNum = initDealM.OriginalAuthSolekNum;
                        inputObj.originalAuthorizationCodeSolek = "7";// initDealM.OriginalAuthorizationCodeSolek;
                    }

                    if (!string.IsNullOrEmpty(initDealM.OriginalAuthNum))
                    {
                        inputObj.originalAuthNum = initDealM.OriginalAuthNum;
                        inputObj.originalAuthorizationCodeManpik = "7";//initDealM.OriginalAuthorizationCodeManpik;
                    }
                }
            }
            else
            {
                inputObj.parameterJ = parameterJValue.ToString();
                inputObj.amount = Math.Round(Convert.ToDecimal(amount) / 100, 2).ToString("00.00").Replace(".", String.Empty);
                ///* "840";//USD   "978";//Euro   "376";//ILS*/
                inputObj.currency = currency;
                inputObj.creditTerms = creditTerms;
                inputObj.tranType = transactionType;
                inputObj.clientInputPan = cardNum;
                inputObj.expirationDate = expDate_YYMM;
                inputObj.panEntryMode = code;
                //CVV
                if (!String.IsNullOrWhiteSpace(cvv2))
                    inputObj.cvv2 = cvv2;

                //תעודת זהות
                if (!String.IsNullOrWhiteSpace(id))
                    inputObj.id = id;

                //מספר אישור
                if (!String.IsNullOrWhiteSpace(authNum))
                {
                    inputObj.authorizationNo = authNum;
                    inputObj.authorizationCodeManpik = "5";
                }


                //תשלומים
                int numOfPaymentI;
                double firstAmountD;
                double nonFirstAmountD;
                if ("89".Contains(creditTerms) && int.TryParse(numOfPayment, out numOfPaymentI) && double.TryParse(firstAmount, out firstAmountD) && double.TryParse(nonFirstAmount, out nonFirstAmountD))//Payments 8 OR 9
                {
                    inputObj.noPayments = numOfPaymentI.ToString();
                    inputObj.firstPayment = Math.Round(Convert.ToDecimal(firstAmountD) / 100, 2).ToString("00.00").Replace(".", String.Empty);
                    inputObj.notFirstPayment = Math.Round(Convert.ToDecimal(nonFirstAmountD) / 100, 2).ToString("00.00").Replace(".", String.Empty);
                }

                //קרדיט
                if ("56".Contains(creditTerms) && int.TryParse(numOfPayment, out numOfPaymentI))//Credit 5 OR 6
                    inputObj.noPayments = numOfPaymentI.ToString();
            }
        }


    }
}
