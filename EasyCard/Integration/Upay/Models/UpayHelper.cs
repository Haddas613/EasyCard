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
        public static MsgModel GetCreateTranMsgModel(string email, string passMd5, bool isTestTerminal, string keyAuthnticate, Shared.Integration.Models.AggregatorCreateTransactionRequest requestTransaction)
        {
            try
            {
                var upaySettings = requestTransaction.AggregatorSettings as UpayTerminalSettings;

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
                    Transfers = new TransfersModel[]
                    {
                        new TransfersModel()
                        {
                            Cellphonenotify = requestTransaction.DealDetails?.ConsumerPhone,
                            Amount = requestTransaction.TransactionAmount.ToString(), //TODO check agurut or shekels,
                            Email = upaySettings?.Email,
                            Numberpayments = requestTransaction.NumberOfInstallments.ToString(),
                            Paymentdate = getCreditDayNumber(requestTransaction.TransactionDate.GetValueOrDefault(DateTime.Today)),
                            Token = requestTransaction.TransactionID,
                            Commissionreduction = "1",
                            Acceptedtransaction = "1",
                            Currency = GetCurrency(requestTransaction.Currency)
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
                    Providername =/* isBitDeal ? "bit" : */"easycardng",
                    Creditcardcompanytype = "ISR",
                    Creditcardtype = "MA"
                };

                msgModel.Request = request;
                return msgModel;
            }
            catch (Exception)
            {
                // TODO: process exception
                throw;
            }
        }

        public static MsgModel GetCommitTranMsgModel(string email, string passMd5, bool isTestTerminal, string keyAuthnticate, Shared.Integration.Models.AggregatorCommitTransactionRequest requestTransaction, Upay.Models.PaymentGatewayAdditionalDetails shvaDetails)
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
                    Returntransfers = new CommitItemModel[]
                     {
                            new CommitItemModel(){
                                 Success = "1",
                                 Cashierid = requestTransaction.AggregatorTransactionID,
                                 Externalid = requestTransaction.TransactionID,
                                 Numberpayments = requestTransaction.NumberOfInstallments.ToString(),
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
                    Providername =/* isBitDeal ? "bit" : */"easycardng",
                };

                msgModel.Request = request;
                return msgModel;
            }
            catch (Exception)
            {
                // TODO: process exception
                throw;
            }
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
                return msgModel;
            }
            catch (Exception)
            {
                // TODO: process exception
                throw;
            }
        }

        public static string GetStringInMD5(string str)
        {
            if (String.IsNullOrEmpty(str))
                return null;

            StringBuilder sb = new StringBuilder();
            try
            {
                using MD5 md5 = new MD5CryptoServiceProvider();
                md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
                var r = md5.Hash;
                foreach (var i in r)
                    sb.Append(i.ToString("x2"));
            }
            catch (Exception)
            {
                // TODO: process exception
                return null;
            }
            return sb.ToString();
        }

        public static Shared.Integration.Models.AggregatorCreateTransactionResponse GetAggregatorCreateTransactionResponse(this Models.TranResponseFullModel operationResponse)
        {
            var response = new UpayCreateTransactionResponse();

            response.Cashierid = operationResponse.Results[1].Result.Transactions[0].Cashierid;
            response.MerchantNumber = operationResponse.Results[1].Result.Transactions[0].Merchantnumber;

            //response.CreditcardCompanycode = operationResponse.CreditcardCompanycode;

            response.SessionId = operationResponse.Results[1].Result.Sessionid;
            response.TotalAmount = operationResponse.Results[1].Result.Transactions[0].Totalamount;
            //response.WebUrl = operationResponse.WebUrl;
            response.Success = operationResponse.Results[1].Success;

            if (!response.Success)
            {
                response.ErrorMessage = operationResponse.Results[1].Header.Errormessage;
                response.ErrorDescription = operationResponse.Results[1].Header.Errordescription;
            }
            return response;
        }

        public static Shared.Integration.Models.AggregatorCommitTransactionResponse GetAggregatorCommitTransactionResponse(this Models.TranResponseFullModel operationResponse)
        {
            var response = new UpayCommitTransactionResponse();

            //Cashierid = operationResponse.Results[1].Result.Transactions[0].Cashierid;

            //response.CreditcardCompanycode = operationResponse.CreditcardCompanycode;

            //response.SessionId = operationResponse.Results[1].Result.Sessionid;
            //response.TotalAmount = operationResponse.Results[1].Result.Transactions[0].Totalamount;
            //response.WebUrl = operationResponse.WebUrl;

            response.Success = operationResponse.Results[1].Header.Errorcode.EndsWith("00") && operationResponse.Results[1].Header.Errormessage.EndsWith("_OK");

            if (!response.Success)
            {
                response.ErrorMessage = String.Format("{0} {1} {2}", operationResponse.Results[1].Header.Errorcode, operationResponse.Results[1].Header.Errormessage, operationResponse.Results[1].Header.Errordescription);
            }
            return response;
        }


        public static Shared.Integration.Models.AggregatorCreateTransactionResponse GetAggregatorCreateTransactionResponse(this Models.OperationResponse operationResponse)
        {
            var response = new UpayCreateTransactionResponse();

            response.Cashierid = operationResponse.Cashierid;
            response.MerchantNumber = operationResponse.MerchantNumber;

            //response.CreditcardCompanycode = operationResponse.CreditcardCompanycode;

            response.SessionId = operationResponse.SessionId;
            response.TotalAmount = operationResponse.TotalAmount;
            //response.WebUrl = operationResponse.WebUrl;
            response.Success = operationResponse.Status == Shared.Api.Models.Enums.StatusEnum.Success;

            if (!response.Success)
            {
                response.ErrorMessage = operationResponse.ErrorMessage;
                response.ErrorDescription = operationResponse.ErrorDescription;
            }
            return response;
        }

        public static Shared.Integration.Models.AggregatorCommitTransactionResponse GetAggregatorCommitTransactionResponse(this Models.OperationResponse operationResponse)
        {
            var response = new UpayCommitTransactionResponse();
            response.Success = operationResponse.Equals("0");

            if (!response.Success)
            {
                response.ErrorMessage = "System Error";
            }

            return response;
        }

        private static string getCreditDayNumber(DateTime transactionDate)
        {
            var credit_day_number = transactionDate.ToString("yyyy-MM-dd");
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

        public static string GetCurrency(Shared.Helpers.CurrencyEnum currencyEnum)
        {
            return currencyEnum switch
            {
                Shared.Helpers.CurrencyEnum.ILS => "NIS",
                Shared.Helpers.CurrencyEnum.USD => "USD",
                Shared.Helpers.CurrencyEnum.EUR => "€",
                _ => string.Empty,
            };
        }

        private static string getUpayExpDateFormat(Shared.Helpers.CardExpiration cardExpiration)
        {
            if (cardExpiration == null)
            {
                return string.Empty;
            }

            return string.Format("{0}{1}/{2}", cardExpiration.Month < 10 ? "0" : string.Empty, cardExpiration.Month, cardExpiration.Year);
        }

        private static DateTime? GetPayDate()
        {
            int dayToday = DateTime.Today.Day;
            int addPay = -1;
            int addMonth = 1;

            if (1 <= dayToday && 15 >= dayToday)
            {
                addPay = 2;
            }
            else if (16 <= dayToday && 31 >= dayToday)
            {
                addPay = 8;
            }

            DateTime dt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, addPay);
            return dt.AddMonths(addMonth);
        }

        private static string GetUpayCardType(string cardVendor)
        {
            Enum.TryParse(cardVendor, out Shared.Integration.Models.CardVendorEnum vendor);
            return ((int)vendor).ToString();
        }
    }
}
