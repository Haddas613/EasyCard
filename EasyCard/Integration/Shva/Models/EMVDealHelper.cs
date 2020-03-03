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
        public static void InitInputObj( InitInputObjRequest req, ref clsInput inputObj)
        {
            int paramJInt = (int)req.ParamJ;
            bool notvalidParamJ = paramJInt != (int)ParamJEnum.MakeDeal && paramJInt != (int)ParamJEnum.J5Deal && paramJInt != (int)ParamJEnum.Check;
            int parameterJValue = notvalidParamJ ? (int)ParamJEnum.MakeDeal : (int)req.ParamJ;//J4 is default


            //initialization deal
            if ("11".Equals(req.TransactionType))
            {
                inputObj.panEntryMode = req.Code;
                inputObj.clientInputPan = req.CardNum;
                inputObj.expirationDate =req.ExpDate_YYMM;


                if (req.IsNewInitDeal)
                {
                    //CVV
                    if (!string.IsNullOrWhiteSpace(req.Cvv2))
                    {
                        inputObj.cvv2 = req.Cvv2;
                    }

                    //תעודת זהות
                    if (!string.IsNullOrWhiteSpace(req.Id))
                    {
                        inputObj.id = req.Id;
                    }

                    inputObj.parameterJ = parameterJValue.ToString();
                    inputObj.creditTerms = req.CreditTerms;
                    inputObj.tranType = req.TransactionType;

                    inputObj.amount = "1";
                    inputObj.stndOrdrFreq = "4";
                    inputObj.stndOrdrTotalNo = "999";
                    inputObj.stndOrdrNo = "0";
                }
                else if (req.InitDealM != null)
                {
                    //חיוב עסקת הוראת קבע אחרי אתחול
                    inputObj.creditTerms = req.CreditTerms;
                    inputObj.tranType = req.TransactionType;
                    inputObj.amount = Math.Round(Convert.ToDecimal(req.Amount) / 100, 2).ToString("00.00").Replace(".", string.Empty);

                    inputObj.originalUid = req.InitDealM.OriginalUid;
                    inputObj.originalTranDate = req.InitDealM.OriginalTranDate;
                    inputObj.originalTranTime = req.InitDealM.OriginalTranTime;
                    inputObj.stndOrdrTotalNo = "999";
                    inputObj.originalAmount = req.InitDealM.Amount;
                    inputObj.stndOrdrNo = req.InitDealM.DealsCounter.ToString();
                    if (!string.IsNullOrEmpty(req.InitDealM.OriginalAuthSolekNum))
                    {
                        inputObj.originalAuthSolekNum = req.InitDealM.OriginalAuthSolekNum;
                        inputObj.originalAuthorizationCodeSolek = "7";// initDealM.OriginalAuthorizationCodeSolek;
                    }

                    if (!string.IsNullOrEmpty(req.InitDealM.OriginalAuthNum))
                    {
                        inputObj.originalAuthNum = req.InitDealM.OriginalAuthNum;
                        inputObj.originalAuthorizationCodeManpik = "7";//initDealM.OriginalAuthorizationCodeManpik;
                    }
                }
            }
            else
            {
                inputObj.parameterJ = parameterJValue.ToString();
                inputObj.amount = Math.Round(Convert.ToDecimal(req.Amount) / 100, 2).ToString("00.00").Replace(".", string.Empty);
                ///* "840";//USD   "978";//Euro   "376";//ILS*/
                inputObj.currency = req.Currency;
                inputObj.creditTerms = req.CreditTerms;
                inputObj.tranType = req.TransactionType;
                inputObj.clientInputPan =req.CardNum;
                inputObj.expirationDate = req.ExpDate_YYMM;
                inputObj.panEntryMode =req.Code;
                //CVV
                if (!string.IsNullOrWhiteSpace(req.Cvv2))
                {
                    inputObj.cvv2 = req.Cvv2;
                }

                //תעודת זהות
                if (!string.IsNullOrWhiteSpace(req.Id))
                {
                    inputObj.id = req.Id;
                }

                //מספר אישור
                if (!string.IsNullOrWhiteSpace(req.AuthNum))
                {
                    inputObj.authorizationNo = req.AuthNum;
                    inputObj.authorizationCodeManpik = "5";
                }


                //תשלומים
                int numOfPaymentI;
                double firstAmountD;
                double nonFirstAmountD;
                if ("89".Contains(req.CreditTerms) && int.TryParse(req.NumOfPayment, out numOfPaymentI) && double.TryParse(req.FirstAmount, out firstAmountD) && double.TryParse(req.NonFirstAmount, out nonFirstAmountD))//Payments 8 OR 9
                {
                    inputObj.noPayments = numOfPaymentI.ToString();
                    inputObj.firstPayment = Math.Round(Convert.ToDecimal(firstAmountD) / 100, 2).ToString("00.00").Replace(".", string.Empty);
                    inputObj.notFirstPayment = Math.Round(Convert.ToDecimal(nonFirstAmountD) / 100, 2).ToString("00.00").Replace(".", string.Empty);
                }

                //קרדיט
                if ("56".Contains(req.CreditTerms) && int.TryParse(req.NumOfPayment, out numOfPaymentI))//Credit 5 OR 6
                {
                    inputObj.noPayments = numOfPaymentI.ToString();
                }
            }
        }
    }
}
