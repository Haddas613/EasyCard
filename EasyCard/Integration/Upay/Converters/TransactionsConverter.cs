using Shared.Api.Models.Enums;
using Shared.Helpers;
using Shared.Integration.Models;
using System;

namespace Upay.Converters
{
    public static class TransactionsConverter
    {
        public static string GetCurrency(CurrencyEnum currencyEnum)
        {
            return currencyEnum switch
            {
                CurrencyEnum.ILS => "NIS",
                CurrencyEnum.USD => "USD",
                CurrencyEnum.EUR => "€",
                _ => string.Empty,
            };
        }
        public static Models.CreateTransactionRequest GetCreateTransactionRequest(this Shared.Integration.Models.AggregatorCreateTransactionRequest createTransactionRequest, UpayGlobalSettings configuration)
        {
            var upayRequest = new Models.CreateTransactionRequest();
            upayRequest.Amount = createTransactionRequest.TransactionAmount.ToString();//TODO check agurut or shekels
            upayRequest.AcceptedTransaction = "1";//TODO
            upayRequest.CellPhone = createTransactionRequest.DealDetails.ConsumerPhone;
            upayRequest.CommissionReduction = "1"; //TODO
            upayRequest.Currency = GetCurrency(createTransactionRequest.Currency);
            upayRequest.NumberPayments = createTransactionRequest.NumberOfInstallments.ToString();
            upayRequest.PaymentDate = createTransactionRequest.TransactionDate.ToString();
            upayRequest.TransactionType = createTransactionRequest.TransactionType;
            var upaySettings = createTransactionRequest.AggregatorSettings as UpayTerminalSettings;
            upayRequest.EmailUser = upaySettings.Email;
            return upayRequest;
        }

        public static Models.CommitTransactionRequest GetCommitTransactionRequest(this Shared.Integration.Models.AggregatorCommitTransactionRequest commitTransactionRequest, UpayGlobalSettings configuration)
        {
            var upayRequest = new Models.CommitTransactionRequest();

            //upayRequest.Amount = commitTransactionRequest.TransactionDetails.Amount;
            //upayRequest.Cardcompany = GetUpayCardType(commitTransactionRequest.CreditCardDetails.CardVendor);// commitTransactionRequest.TransactionDetails.CardCompany;
          //  upayRequest.Cardnumber = commitTransactionRequest.CreditCardDetails.CardLastFourDigits;
           // upayRequest.Cardexpdate = getUpayExpDateFormat(commitTransactionRequest.CreditCardDetails.CardExpiration);
            //upayRequest.Cardtype = GetUpayCardType(commitTransactionRequest.CreditCardDetails.CardBrand);
            upayRequest.Cashierid = commitTransactionRequest.AggregatorTransactionID;
           // upayRequest.Cellphonenotify = commitTransactionRequest.TransactionDetails.OwnerPhoneNumber;
            //upayRequest.Code = "000";//TODO CONST FOR SUCCESS

            var shvaDetails = commitTransactionRequest.ProcessorTransactionDetails as Upay.Models.PaymentGatewayAdditionalDetails;
           // upayRequest.Dealnumber = shvaDetails.ShvaShovarNumber;// PaymentGatewayAdditionalDetails = shvaDetails;
            //upayRequest.Foreigncard = "False";//TODO
            //upayRequest.Identitynum = commitTransactionRequest.TransactionDetails.OwnerIdentityNumber;
            //upayRequest.Oknumber = shvaDetails.ShvaAuthNum;
            //DateTime? payDate = GetPayDate();
            //upayRequest.Paydate = payDate.HasValue ? payDate.Value.ToString("dd/MM/yyyy") : string.Empty;
            //upayRequest.Payments = commitTransactionRequest.TransactionDetails.PaymentsNumber;
            //upayRequest.Sessionid = commitTransactionRequest.CorrelationId;
            //upayRequest.Sixdigits = commitTransactionRequest.CreditCardDetails.CardBin;
            upayRequest.Token = commitTransactionRequest.TransactionDetails.ExternalID;// externalID newguid TODO


            //response.IsTourist TODO:
            //details.Solek TODO:

            return upayRequest;
        }

        private static string GetUpayCardType(string cardVendor)
        {
            Enum.TryParse(cardVendor, out CardVendorEnum vendor);
            return ((int)vendor).ToString();
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
           // response.TotalAmount = operationResponse.Results[1].Result.Transactions[0].Totalamount;
            //response.WebUrl = operationResponse.WebUrl;
            response.Success = operationResponse.Results[1].Success;

            if (!response.Success)
            {
                response.ErrorMessage = operationResponse.Results[1].Header.Errormessage;
                //response. = operationResponse.Results[1].Header.Errordescription;
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
            response.Success = operationResponse.Status == StatusEnum.Success;

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

       /* public static Shared.Integration.Models.AggregatorCommitTransactionResponse GetAggregatorCommitTransactionResponse(this string operationResponse)
        {
            var response = new UpayCommitTransactionResponse();
            response.Success = operationResponse.Equals("0");

            if (!response.Success)
            {
                response.ErrorMessage = "System Error";
            }

            return response;
        }
        */
        private static string getUpayExpDateFormat(CardExpiration cardExpiration)
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

            return null;
        }

    }
}
