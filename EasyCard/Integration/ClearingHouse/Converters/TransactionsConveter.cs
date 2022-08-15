using AutoMapper;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClearingHouse.Converters
{
    public static class TransactionsConveter
    {
        public static Models.CreateTransactionRequest GetCreateTransactionRequest(this Shared.Integration.Models.AggregatorCreateTransactionRequest createTransactionRequest, ClearingHouseGlobalSettings configuration)
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
            details.CardExpiration = createTransactionRequest.CreditCardDetails.CardExpiration == null ? string.Empty : createTransactionRequest.CreditCardDetails.CardExpiration.ToString();

            //response.IsTourist TODO:
            //details.Solek TODO:

            chRequest.PaymentGatewayID = configuration.PaymentGatewayID;

            details.CardOwnerName = createTransactionRequest.CreditCardDetails.CardOwnerName;
            details.CardOwnerNationalId = createTransactionRequest.CreditCardDetails.CardOwnerNationalID?.Trim();
            details.CreditCardVendor = createTransactionRequest.CreditCardDetails.CardVendor;

            details.ConsumerEmail = createTransactionRequest.DealDetails?.ConsumerEmail;
            details.ConsumerPhone = createTransactionRequest.DealDetails?.ConsumerPhone;
            details.DealDescription = createTransactionRequest.DealDetails?.DealDescription;

            details.DealReference = createTransactionRequest.TransactionID;

            var clearingHouseSettings = createTransactionRequest.AggregatorSettings as ClearingHouseTerminalSettings;

            details.TerminalReference = clearingHouseSettings.ShvaTerminalReference; // TODO: this is temporary implementation //createTransactionRequest.ProcessorTerminalID;

            details.MerchantReference = clearingHouseSettings.MerchantReference;

            details.TransactionDate = createTransactionRequest.TransactionDate;

            details.TransactionType = GetTransactionType(createTransactionRequest.TransactionType, createTransactionRequest.SpecialTransactionType == Shared.Integration.Models.SpecialTransactionTypeEnum.Refund);

            chRequest.PaymentGatewayTransactionDetails = details;

            chRequest.IsBit = createTransactionRequest.IsBit;

            return chRequest;
        }

        public static Models.CommitTransactionRequest GetCommitTransactionRequest(this Shared.Integration.Models.AggregatorCommitTransactionRequest commitTransactionRequest, ClearingHouseGlobalSettings configuration, IMapper mapper)
        {
            var chRequest = new Models.CommitTransactionRequest();

            chRequest.ConcurrencyToken = commitTransactionRequest.ConcurrencyToken;

            chRequest.CardBin = commitTransactionRequest.CreditCardDetails.CardBin;
            chRequest.CardLastFourDigits = commitTransactionRequest.CreditCardDetails.CardLastFourDigits;
            chRequest.CreditCardVendor = commitTransactionRequest.CreditCardDetails.CardVendor;
            chRequest.DealReference = commitTransactionRequest.TransactionID;

            //response.IsTourist TODO:
            //details.Solek TODO:

            chRequest.PaymentGatewayID = configuration.PaymentGatewayID;
            chRequest.PaymentGatewayAdditionalDetails = mapper.Map<Models.PaymentGatewayAdditionalDetails>(commitTransactionRequest.ProcessorTransactionDetails);

            chRequest.Solek = (int)chRequest.PaymentGatewayAdditionalDetails.Solek;
            chRequest.IsTourist = commitTransactionRequest.CreditCardDetails.CardVendor == Shared.Integration.Models.CardVendorEnum.UNKNOWN.ToString();

            chRequest.IsBit = commitTransactionRequest.IsBit;

            return chRequest;
        }

        public static Shared.Integration.Models.AggregatorCreateTransactionResponse GetAggregatorCreateTransactionResponse(this Models.OperationResponse operationResponse)
        {
            var response = new ClearingHouseCreateTransactionResponse();

            response.CorrelationID = operationResponse.CorrelationId;

            response.ClearingHouseTransactionID = operationResponse.EntityID;

            response.Success = operationResponse.Status == Models.StatusEnum.Success;

            response.ConcurrencyToken = operationResponse.ConcurrencyToken;

            if (!response.Success)
            {
                response.ErrorMessage = operationResponse.Message;

                if (operationResponse.Errors?.Count > 0)
                {
                    response.Errors = operationResponse.Errors.Select(d => new Error { Code = d.Code, Description = d.Description }).ToList();
                }
            }

            return response;
        }

        public static Shared.Integration.Models.AggregatorTransactionResponse GetAggregatorTransactionResponse(this Models.TransactionResponse operationResponse)
        {
            var response = new ClearingHouseTransactionResponse();

            response.ClearingHouseTransactionID = operationResponse.TransactionID;

            response.ConcurrencyToken = operationResponse.ConcurrencyToken;

            return response;
        }

        public static Shared.Integration.Models.AggregatorCommitTransactionResponse GetAggregatorCommitTransactionResponse(this Models.OperationResponse operationResponse)
        {
            var response = new ClearingHouseCommitTransactionResponse();

            response.CorrelationID = operationResponse.CorrelationId;

            response.Success = operationResponse.Status == Models.StatusEnum.Success;

            if (!response.Success)
            {
                response.ErrorMessage = operationResponse.Message;

                if (operationResponse.Errors?.Count > 0)
                {
                    response.Errors = operationResponse.Errors.Select(d => new Error { Code = d.Code, Description = d.Description }).ToList();
                }
            }

            return response;
        }

        public static Models.RejectTransactionRequest GetCancelTransactionRequest(this Shared.Integration.Models.AggregatorCancelTransactionRequest cancelTransactionRequest, ClearingHouseGlobalSettings configuration)
        {
            return new Models.RejectTransactionRequest
            {
                RejectionReason = cancelTransactionRequest.RejectionReason,
                ConcurrencyToken = cancelTransactionRequest.ConcurrencyToken,
                PaymentGatewayID = configuration.PaymentGatewayID,
            };
        }

        public static Shared.Integration.Models.AggregatorCancelTransactionResponse GetAggregatorCancelTransactionResponse(this Models.OperationResponse operationResponse)
        {
            var response = new ClearingHouseCancelTransactionResponse();

            response.CorrelationID = operationResponse.CorrelationId;

            response.Success = operationResponse.Status == Models.StatusEnum.Success;

            if (!response.Success)
            {
                response.ErrorMessage = operationResponse.Message;

                if (operationResponse.Errors?.Count > 0)
                {
                    response.Errors = operationResponse.Errors.Select(d => new Error { Code = d.Code, Description = d.Description }).ToList();
                }
            }

            return response;
        }

        private static Models.TransactionTypeEnum GetTransactionType(Shared.Integration.Models.TransactionTypeEnum transactionType, bool isRefund)
        {
            if (isRefund)
            {
                return Models.TransactionTypeEnum.Refund;
            }

            switch (transactionType)
            {
                case Shared.Integration.Models.TransactionTypeEnum.Credit:
                    return Models.TransactionTypeEnum.Credit;
                case Shared.Integration.Models.TransactionTypeEnum.Installments:
                    return Models.TransactionTypeEnum.Installments;
                case Shared.Integration.Models.TransactionTypeEnum.RegularDeal:
                    return Models.TransactionTypeEnum.Regular;
                case Shared.Integration.Models.TransactionTypeEnum.Immediate:
                    return Models.TransactionTypeEnum.Regular; //TODO: immediate type
                default:
                    throw new Exception($"Cannot convert transaction type {transactionType} to Clearing House transaction type"); // TODO:
            }
        }
    }
}
