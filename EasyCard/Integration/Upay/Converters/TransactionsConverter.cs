using Shared.Api.Models.Enums;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Upay;

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

            var upaySettings = createTransactionRequest.AggregatorSettings as UpayTerminalSettings;
            upayRequest.EmailUser = upaySettings.Email;
            return upayRequest;
        }

          public static Shared.Integration.Models.AggregatorCreateTransactionResponse GetAggregatorCreateTransactionResponse(this Models.OperationResponse operationResponse)
        {
            var response = new UpayCreateTransactionResponse();

            response.Cashierid = operationResponse.Cashierid;

            response.CreditcardCompanycode = operationResponse.CreditcardCompanycode;

            response.ErrorDescription = operationResponse.ErrorDescription;
            response.MerchantNumber = operationResponse.ErrorMessage;
            response.SessionId = operationResponse.SessionId;
            response.TotalAmount = operationResponse.TotalAmount;
            response.WebUrl = operationResponse.WebUrl;


            response.Success = operationResponse.Status == StatusEnum.Success;

            if (!response.Success)
            {

                response.ErrorMessage = operationResponse.ErrorMessage;

                if (operationResponse.Errors?.Count > 0)
                {
                    //response.Errors = operationResponse.Errors.Select(d => new Error { Code = d.Code, Description = d.Description }).ToList(); TODO
                }
            }

            return response;
        }


    }
}
