using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Upay;

namespace Upay.Models
{
    public static class UpayHelper
    {
        public static MsgModel GetCreateTranMsgModel(string email, string passMd5, bool isTestTerminal, string keyAuthnticate, CreateTransactionRequest requestTransaction)
        {
            try
            {
                MsgModel msgModel = new MsgModel();
                HeaderModel header = new HeaderModel();
                header.language = "HE";
                header.livesystem = isTestTerminal ? 0 : 1;
                header.refername = "UPAY";
                msgModel.Header = header;

                RequestModel request = new RequestModel();
                request.Encoding = "json";
                request.Mainaction = "CASHIER";
                request.Minoraction = "REDIRECTDEPOSITCREDITCARDTRANSFER";
                request.Numbertemplate = 1;
                request.Parameters = new CreateTranModel()
                {
                    Transfers = new TransfersModel[]{
                            new TransfersModel(){
                                Cellphonenotify = requestTransaction.CellPhone,
                                Amount = requestTransaction.Amount,
                                Email= requestTransaction.EmailUser,
                                Numberpayments =requestTransaction.NumberPayments,
                                Paymentdate = getCreditDayNumber(),
                                Token = requestTransaction.Token,
                                Commissionreduction = requestTransaction.CommissionReduction,
                                Acceptedtransaction = requestTransaction.AcceptedTransaction,
                                Currency = requestTransaction.Currency
                               /* 
                                *  for the future BIT
                                *  Ipnurl =  ! requestTransaction. ? String.Empty :
                               (String.IsNullOrWhiteSpace(formID) ?String.Format("{0}/RestAPI/api/UpayGetBitResult/{1}" ,System.Configuration.ConfigurationManager.AppSettings["REDIRECT_BASE_URL"], recordIDTermDeal) : String.Format("{0}/RestAPI/api/UpayGetBitResult/{1}" ,System.Configuration.ConfigurationManager.AppSettings["REDIRECT_BASE_URL"], formID)),
                                returnurl = ! isBitDeal  ? String.Empty :
                               ( !String.IsNullOrWhiteSpace(formID)? (!String.IsNullOrWhiteSpace(upayReturnUrl) ?  upayReturnUrl : String.Format("{0}/BillingForm/ShovarOK?dealID=-1&Token={1}" ,System.Configuration.ConfigurationManager.AppSettings["REDIRECT_BASE_URL"], formID))
                               : (!String.IsNullOrWhiteSpace(upayReturnUrl) ?  upayReturnUrl : String.Format("{0}/BillingForm/ShovarOK?dealID=-1&Token={1}" ,System.Configuration.ConfigurationManager.AppSettings["REDIRECT_BASE_URL"], recordIDTermDeal)))*/
                            }
                        },
                    Passwordmd5 = passMd5,
                    Key = keyAuthnticate,
                    //cardreader = (Convert.ToInt32(model.DealType)) == 0 ? "1" : "0", /// קורא כרטיס
                    Cardreader = "0",
                    Providername =/* isBitDeal ? "bit" : */"easycard",
                    Creditcardcompanytype = "ISR",
                    Creditcardtype = "MA"
                };

                msgModel.Request = request;
                return msgModel;
            }
            catch (Exception ex)
            {
                //Logger.Write(ex, "Errors"); TODO
            }
            return new MsgModel();
        }

        public static MsgModel GetCommitTranMsgModel(string email, string passMd5, bool isTestTerminal, string keyAuthnticate, CommitTransactionRequest requestTransaction)
        {
            try
            {
                MsgModel msgModel = new MsgModel();
                HeaderModel header = new HeaderModel();
                header.language = "HE";
                header.livesystem = isTestTerminal ? 0 : 1;
                header.refername = "UPAY";
                msgModel.Header = header;

                RequestModel request = new RequestModel();
                request.Encoding = "json";
                request.Mainaction = "CASHIER";
                request.Minoraction = "RETURNDEPOSITCREDITCARDTRANSFER";
                request.Parameters = new CommitTranModel()
                {
                     Returntransfers = new CommitItemModel[]{
                            new CommitItemModel(){
                                 Success = "1",
                                  Cashierid = requestTransaction.Cashierid,
                                   Externalid = requestTransaction.Token,
                                    Numberpayments = requestTransaction.NumberPayments,
                                //Cellphonenotify = requestTransaction.CellPhone,
                                //Amount = requestTransaction.Amount,
                                //Email= requestTransaction.EmailUser,
                                //Numberpayments =requestTransaction.NumberPayments,
                                //Paymentdate = getCreditDayNumber(),
                                //Token = requestTransaction.Token,
                                //Commissionreduction = requestTransaction.CommissionReduction,
                                //Acceptedtransaction = requestTransaction.AcceptedTransaction,
                                //Currency = requestTransaction.Currency
                               /* 
                                *  for the future BIT
                                *  Ipnurl =  ! requestTransaction. ? String.Empty :
                               (String.IsNullOrWhiteSpace(formID) ?String.Format("{0}/RestAPI/api/UpayGetBitResult/{1}" ,System.Configuration.ConfigurationManager.AppSettings["REDIRECT_BASE_URL"], recordIDTermDeal) : String.Format("{0}/RestAPI/api/UpayGetBitResult/{1}" ,System.Configuration.ConfigurationManager.AppSettings["REDIRECT_BASE_URL"], formID)),
                                returnurl = ! isBitDeal  ? String.Empty :
                               ( !String.IsNullOrWhiteSpace(formID)? (!String.IsNullOrWhiteSpace(upayReturnUrl) ?  upayReturnUrl : String.Format("{0}/BillingForm/ShovarOK?dealID=-1&Token={1}" ,System.Configuration.ConfigurationManager.AppSettings["REDIRECT_BASE_URL"], formID))
                               : (!String.IsNullOrWhiteSpace(upayReturnUrl) ?  upayReturnUrl : String.Format("{0}/BillingForm/ShovarOK?dealID=-1&Token={1}" ,System.Configuration.ConfigurationManager.AppSettings["REDIRECT_BASE_URL"], recordIDTermDeal)))*/
                            }
                        },
                    Email = email,
                    Passwordmd5 = passMd5,
                    Key = keyAuthnticate,
                    //cardreader = (Convert.ToInt32(model.DealType)) == 0 ? "1" : "0", /// קורא כרטיס
                    Providername =/* isBitDeal ? "bit" : */"easycard",
                };

                msgModel.Request = request;
                return msgModel;
            }
            catch (Exception ex)
            {
                //Logger.Write(ex, "Errors"); TODO
            }
            return new MsgModel();
        }

        public static MsgModel GetLoginMsgModel(string email, string passMd5, bool isTestTerminal, string keyAuthnticate)
        {
            try
            {
                MsgModel msgModel = new MsgModel();
                HeaderModel header = new HeaderModel();
                header.language = "HE";
                header.livesystem = isTestTerminal ? 0 : 1;
                header.refername = "UPAY";
                msgModel.Header = header;

                RequestModel request = new RequestModel();
                request.Encoding = "json";
                request.Mainaction = "CONNECTION";
                request.Minoraction = "LOGIN";
                request.Numbertemplate = 1;

                LoginModel param1 = new LoginModel();
                param1.Email = email;
                if (!String.IsNullOrEmpty(keyAuthnticate))
                {
                    param1.Key = keyAuthnticate;
                }
                else
                {
                    param1.Passwordmd5 = passMd5;
                }
                request.Parameters = param1;
                msgModel.Request = request;
                return  msgModel;
            }
            catch (Exception ex)
            {
                //Logger.Write(ex, "Errors"); TODO
            }
            return new   MsgModel();
        }

        public static string GetStringInMD5(string str)
        {
            if (String.IsNullOrEmpty(str))
                return null;

            StringBuilder sb = new StringBuilder();
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
                var r = md5.Hash;
                foreach (var i in r)
                    sb.Append(i.ToString("x2"));
            }
            catch (Exception ex)
            {
                return null;
            }
            return sb.ToString();
        }

        private static string getCreditDayNumber()
        {
            var credit_day_number = DateTime.Now.ToString("yyyy-MM-dd");
            /*
           switch (billingModel.TransactionType)
           {
              case  Shared.Integration.Models.TransactionTypeEnum. .SHOTEF_30_UPAY:
                   var month = DateTime.Now.Month + 1;
                   var year = DateTime.Now.Year;
                   var date = year + "-" + month + "-28";
                   credit_day_number = "2015-02-28";
                   break;
               case DealTypeEnum.SHOTEF_60_UPAY:
                   var month1 = DateTime.Now.Month + 2;
                   var year1 = DateTime.Now.Year;
                   var date1 = year1 + "-" + month1 + "-28";
                   credit_day_number = "2015-03-28"; ;
                   break;
               case DealTypeEnum.SHOTEF_90_UPAY:
                   var month2 = DateTime.Now.Month + 3;
                   var year2 = DateTime.Now.Year;
                   var date2 = year2 + "-" + month2 + "-28";
                   credit_day_number = "2015-04-28"; ;
                   break;
               default:
                   Console.WriteLine("Default case");
                   break;
           }
           */
            return credit_day_number;
        }
    }
}
