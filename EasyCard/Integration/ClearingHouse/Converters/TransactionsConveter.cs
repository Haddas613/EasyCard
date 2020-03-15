using System;
using System.Collections.Generic;
using System.Text;

namespace ClearingHouse.Converters
{
    public static class TransactionsConveter
    {
        public static Models.CreateTransactionRequest GetCreateTransactionRequest(this Shared.Integration.Models.AggregatorCreateTransactionRequest createTransactionRequest)
        {
            var chRequest = new Models.CreateTransactionRequest();

            chRequest.CardNotPresent = createTransactionRequest.CardPresence == Shared.Integration.Models.CardPresenceEnum.CardNotPresent;
            chRequest.Currency = createTransactionRequest.Currency.ToString();
            chRequest.InitialPaymentAmount = createTransactionRequest.InitialPaymentAmount;
            chRequest.InstallmentPaymentAmount = createTransactionRequest.InstallmentPaymentAmount;
            chRequest.Payments = createTransactionRequest.NumberOfInstallments;
            chRequest.TotalAmount = createTransactionRequest.TotalAmount;

            chRequest.EasyCardTerminalReference = createTransactionRequest.EasyCardTerminalID;

            var details = new Models.PaymentGatewayTransactionDetails();

            details.CardBin = createTransactionRequest.CreditCardDetails.CardBin;
            details.CardLastFourDigits = createTransactionRequest.CreditCardDetails.CardLastFourDigits;
            details.CardExpiration = createTransactionRequest.CreditCardDetails.CardExpiration.ToString();

            //response.IsTourist TODO:
            //details.Solek TODO:
            //response.PaymentGatewayID TODO:

            details.CardOwnerName = createTransactionRequest.CreditCardDetails.CardOwnerName;
            details.CardOwnerNationalId = createTransactionRequest.CreditCardDetails.CardOwnerNationalId;
            details.CreditCardVendor = createTransactionRequest.CreditCardDetails.CardVendor;

            details.ConsumerEmail = createTransactionRequest.DealDetails.ConsumerEmail;
            details.ConsumerPhone = createTransactionRequest.DealDetails.ConsumerPhone;
            details.DealDescription = createTransactionRequest.DealDetails.DealDescription;

            details.DealReference = createTransactionRequest.TransactionID;

            details.TerminalReference = createTransactionRequest.ProcessorTerminalID;

            var clearingHouseSettings = createTransactionRequest.AggregatorSettings as ClearingHouseTerminalSettings;

            details.MerchantReference = clearingHouseSettings.MerchantReference;

            details.TransactionDate = createTransactionRequest.TransactionDate;

            details.TransactionType = GetTransactionType(createTransactionRequest.TransactionType);

            chRequest.PaymentGatewayTransactionDetails = details;

            return chRequest;
        }

        public static Models.CommitTransactionRequest GetCommitTransactionRequest(this Shared.Integration.Models.AggregatorCommitTransactionRequest commitTransactionRequest)
        {
            var response = new Models.CommitTransactionRequest();

            response.CardBin = commitTransactionRequest.CreditCardDetails.CardBin;
            response.CardLastFourDigits = commitTransactionRequest.CreditCardDetails.CardLastFourDigits;
            response.CreditCardVendor = commitTransactionRequest.CreditCardDetails.CardVendor;
            response.DealReference = commitTransactionRequest.TransactionID;

            //response.IsTourist TODO:
            //details.Solek TODO:
            //response.PaymentGatewayID TODO:

            var shvaDetails = commitTransactionRequest.ProcessorTransactionDetails as Models.PaymentGatewayAdditionalDetails;

            response.PaymentGatewayAdditionalDetails = shvaDetails;

            return response;
        }

        public static Shared.Integration.Models.AggregatorCreateTransactionResponse GetAggregatorCreateTransactionResponse(this Models.OperationResponse operationResponse)
        {
            var response = new Shared.Integration.Models.AggregatorCreateTransactionResponse();

            // TODO:

            return response;
        }

        public static Shared.Integration.Models.AggregatorCommitTransactionResponse GetAggregatorCommitTransactionResponse(this Models.OperationResponse operationResponse)
        {
            var response = new Shared.Integration.Models.AggregatorCommitTransactionResponse();

            // TODO:

            return response;
        }

        private static Models.TransactionTypeEnum GetTransactionType(Shared.Integration.Models.TransactionTypeEnum transactionType)
        {
            switch (transactionType)
            {
                case Shared.Integration.Models.TransactionTypeEnum.Credit:
                    return Models.TransactionTypeEnum.Credit;
                case Shared.Integration.Models.TransactionTypeEnum.FirstInstallment:
                    return Models.TransactionTypeEnum.Installments;
                case Shared.Integration.Models.TransactionTypeEnum.Refund:
                    return Models.TransactionTypeEnum.Refund;
                case Shared.Integration.Models.TransactionTypeEnum.RegularDeal:
                    return Models.TransactionTypeEnum.Regular;
                default:
                    throw new Exception($"Cannot convert transaction type {transactionType} to Clearing House transaction type"); // TODO:
            }
        }
    }
}
