﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Api;
using Shared.Api.Configuration;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Email;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Queue;
using Shared.Helpers.Security;
using Shared.Helpers.Templating;
using Shared.Integration.Exceptions;
using Shared.Integration.Models;
using Swashbuckle.AspNetCore.Filters;
using Transactions.Api.Extensions;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Billing;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Api.Swagger;
using Transactions.Api.Validation;
using Transactions.Api.Validation.Options;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared;
using Transactions.Shared.Enums;
using Z.EntityFramework.Plus;
using Merchants.Business.Entities.Terminal;
using Shared.Integration.ExternalSystems;
using Shared.Integration;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;
using Shared.Helpers.Services;
using SharedBusiness = Shared.Business;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Controllers
{
    public partial class TransactionsApiController
    {
        private async Task<ActionResult<OperationResponse>> ProcessTransaction(CreateTransactionRequest model, CreditCardTokenKeyVault token, JDealTypeEnum jDealType = JDealTypeEnum.J4, SpecialTransactionTypeEnum specialTransactionType = SpecialTransactionTypeEnum.RegularDeal, Guid? initialTransactionID = null, BillingDeal billingDeal = null, Guid? paymentRequestID = null)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            if (model.PinPad == true && string.IsNullOrWhiteSpace(model.PinPadDeviceID))
            {
                var nayaxIntegration = EnsureExists(terminal.Integrations.FirstOrDefault(ex => ex.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID));
                var devices = nayaxIntegration.Settings.ToObject<Nayax.NayaxTerminalCollection>();
                var firstDevice = devices.devices.FirstOrDefault();

                if (firstDevice == null)
                {
                    throw new EntityNotFoundException(SharedBusiness.Messages.ApiMessages.EntityNotFound, "PinPadDevice", null);
                }

                model.PinPadDeviceID = firstDevice.TerminalID;
            }

            TransactionTerminalSettingsValidator.Validate(terminal.Settings, model, token, jDealType, specialTransactionType, initialTransactionID);

            //TODO: map terminal to model?
            if (model.VATRate == null)
            {
                model.VATRate = terminal.Settings.VATRate;
            }

            var transaction = mapper.Map<PaymentTransaction>(model);

            // NOTE: this is security assignment
            mapper.Map(terminal, transaction);

            bool pinpadDeal = model.PinPad ?? false;

            transaction.SpecialTransactionType = specialTransactionType;
            transaction.JDealType = jDealType;
            transaction.BillingDealID = billingDeal?.BillingDealID;
            transaction.InitialTransactionID = initialTransactionID;
            transaction.DocumentOrigin = GetDocumentOrigin(billingDeal?.BillingDealID, paymentRequestID, pinpadDeal);
            transaction.PaymentRequestID = paymentRequestID;

            if (paymentRequestID == null)
            {
                transaction.PaymentIntentID = model.PaymentIntentID;
            }

            transaction.CardPresence = pinpadDeal ? CardPresenceEnum.Regular : model.CardPresence;

            if (transaction.DealDetails == null)
            {
                transaction.DealDetails = new Business.Entities.DealDetails();
            }

            if (model.IssueInvoice == true && model.InvoiceDetails == null)
            {
                //TODO: Handle refund & credit default invoice types
                model.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
            }

            // Update card information based on token
            CreditCardTokenDetails dbToken = null;

            //TODO: check token expiration
            if (token != null)
            {
                if (token.TerminalID != terminal.TerminalID)
                {
                    throw new EntityNotFoundException(SharedBusiness.Messages.ApiMessages.EntityNotFound, "CreditCardToken", null);
                }

                if (token.CardExpiration?.Expired == true)
                {
                    return BadRequest(new OperationResponse($"{Messages.CreditCardExpired}", StatusEnum.Error, model.CreditCardToken));
                }

                mapper.Map(token, transaction.CreditCardDetails);
                mapper.Map(token, transaction);

                dbToken = EnsureExists(await creditCardTokenService.GetTokens().FirstOrDefaultAsync(d => d.CreditCardTokenID == model.CreditCardToken));

                if (transaction.InitialTransactionID == null)
                {
                    transaction.InitialTransactionID = dbToken.InitialTransactionID;
                }

                if (transaction.DealDetails?.ConsumerID == null)
                {
                    transaction.DealDetails.ConsumerID = dbToken.ConsumerID;
                }
            }
            else
            {
                mapper.Map(model.CreditCardSecureDetails, transaction.CreditCardDetails);
            }

            // Check consumer
            var consumer = transaction.DealDetails.ConsumerID != null ? EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == transaction.DealDetails.ConsumerID && d.TerminalID == terminal.TerminalID), "Consumer") : null;

            if (consumer != null)
            {
                if (dbToken != null)
                {
                    if (consumer.ConsumerID != dbToken.ConsumerID)
                    {
                        throw new EntityNotFoundException(SharedBusiness.Messages.ApiMessages.EntityNotFound, "CreditCardToken", null);
                    }

                    // NOTE: consumer can pay using another person credit card

                    //if (!string.IsNullOrWhiteSpace(consumer.ConsumerNationalID) && !string.IsNullOrWhiteSpace(dbToken.CardOwnerNationalID) && !consumer.ConsumerNationalID.Equals(dbToken.CardOwnerNationalID, StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    throw new EntityConflictException(Transactions.Shared.Messages.ConsumerNatIdIsNotEqTranNatId, "Consumer");
                    //}
                }
                else
                {
                    // NOTE: consumer can pay using another person credit card

                    //if (!string.IsNullOrWhiteSpace(consumer.ConsumerNationalID) && model.CreditCardSecureDetails != null && !string.IsNullOrWhiteSpace(model.CreditCardSecureDetails.CardOwnerNationalID) && !consumer.ConsumerNationalID.Equals(model.CreditCardSecureDetails.CardOwnerNationalID, StringComparison.InvariantCultureIgnoreCase))
                    //{
                    //    throw new EntityConflictException(Transactions.Shared.Messages.ConsumerNatIdIsNotEqTranNatId, "Consumer");
                    //}
                }
            }

            transaction.DealDetails.CheckConsumerDetails(consumer);
            transaction.Calculate();

            // Update details if needed
            transaction.DealDetails.UpdateDealDetails(consumer, terminal.Settings, transaction, transaction.CreditCardDetails);
            if (model.IssueInvoice.GetValueOrDefault())
            {
                model.InvoiceDetails.UpdateInvoiceDetails(terminal.InvoiceSettings, transaction);
            }

            if (string.IsNullOrWhiteSpace(transaction.CreditCardDetails.CardOwnerName) && consumer != null)
            {
                transaction.CreditCardDetails.CardOwnerName = consumer.ConsumerName;
            }

            // map consumer name from card details if needed
            transaction.DealDetails.UpdateDealDetails(transaction.CreditCardDetails);

            transaction.MerchantIP = GetIP();
            transaction.CorrelationId = GetCorrelationID();

            var terminalAggregator = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Aggregator),
                Transactions.Shared.Messages.AggregatorNotDefined);

            var terminalProcessor = ValidateExists(
                terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.Processor),
                Transactions.Shared.Messages.ProcessorNotDefined);

            TerminalExternalSystem terminalPinpadProcessor = null;

            if (pinpadDeal)
            {
                terminalPinpadProcessor = ValidateExists(
                    terminal.Integrations.FirstOrDefault(t => t.Type == Merchants.Shared.Enums.ExternalSystemTypeEnum.PinpadProcessor),
                    Transactions.Shared.Messages.ProcessorNotDefined);
            }

            transaction.AggregatorID = terminalAggregator.ExternalSystemID;
            transaction.ProcessorID = terminalProcessor.ExternalSystemID;

            var aggregator = aggregatorResolver.GetAggregator(terminalAggregator);
            var processor = processorResolver.GetProcessor(terminalProcessor);
            IProcessor pinpadProcessor = null;
            if (pinpadDeal)
            {
                pinpadProcessor = processorResolver.GetProcessor(terminalPinpadProcessor);
            }

            var aggregatorSettings = aggregatorResolver.GetAggregatorTerminalSettings(terminalAggregator, terminalAggregator.Settings);
            mapper.Map(aggregatorSettings, transaction);

            var processorSettings = processorResolver.GetProcessorTerminalSettings(terminalProcessor, terminalProcessor.Settings);
            mapper.Map(processorSettings, transaction);

            object pinpadProcessorSettings = null;
            if (pinpadDeal)
            {
                if (terminalPinpadProcessor?.Settings != null && terminalPinpadProcessor.ExternalSystemID == ExternalSystemHelpers.NayaxPinpadProcessorExternalSystemID)
                {
                    var devices = terminalPinpadProcessor.Settings.GetValue("devices")?.ToObject<IEnumerable<JObject>>();
                    if (devices != null)
                    {
                        terminalPinpadProcessor.Settings = EnsureExists(
                            devices.FirstOrDefault(d => d.GetValue("terminalID").Value<string>() == model.PinPadDeviceID), "PinPad Terminal");
                    }
                }

                pinpadProcessorSettings = processorResolver.GetProcessorTerminalSettings(terminalPinpadProcessor, terminalPinpadProcessor.Settings);
                mapper.Map(pinpadProcessorSettings, transaction);

                if (!string.IsNullOrEmpty(model.CardOwnerNationalID))
                {
                    transaction.CreditCardDetails.CardOwnerNationalID = model.CardOwnerNationalID;
                }

                if (!string.IsNullOrEmpty(model.CardOwnerName))
                {
                    transaction.CreditCardDetails.CardOwnerName = model.CardOwnerName;
                }
            }

            await transactionsService.CreateEntity(transaction);
            metrics.TrackTransactionEvent(transaction, TransactionOperationCodesEnum.TransactionCreated);

            ProcessorPreCreateTransactionResponse pinpadPreCreateResult = null;
            var processorRequest = mapper.Map<ProcessorCreateTransactionRequest>(transaction);
            if (pinpadDeal)
            {
                try
                {
                    processorRequest.PinPadProcessorSettings = pinpadProcessorSettings;
                    var lastDeal = await GetLastShvaTransactionDetails(transaction.ShvaTransactionDetails.ShvaTerminalID);
                    mapper.Map(lastDeal, processorRequest); // Map details of prev shva transaction

                    //1. check pair device EVT
                    await NotifyStatusChanged(transaction, Messages.ApproveTransactionOnDevice);

                    pinpadPreCreateResult = await pinpadProcessor.PreCreateTransaction(processorRequest);

                    if (!pinpadPreCreateResult.Success)
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToConfirmByProcesor, rejectionMessage: pinpadPreCreateResult.ErrorMessage);

                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.RejectedByProcessor}: {pinpadPreCreateResult.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, pinpadPreCreateResult.Errors));
                    }
                    else
                    {
                        mapper.Map(pinpadPreCreateResult, processorRequest);

                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByPinpadPreProcessor);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Pinpad Processor PreCreate Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToConfirmByProcesor, rejectionMessage: ex.Message);

                    return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, pinpadPreCreateResult?.Errors));
                }
            }

            // create transaction in aggregator (Clearing House)
            if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
            {
                try
                {
                    var aggregatorRequest = mapper.Map<AggregatorCreateTransactionRequest>(transaction);
                    aggregatorRequest.AggregatorSettings = aggregatorSettings;

                    var aggregatorValidationErrorMsg = aggregator.Validate(aggregatorRequest);
                    if (aggregatorValidationErrorMsg != null)
                    {
                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.RejectedByAggregator}: {aggregatorValidationErrorMsg}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
                    }

                    var aggregatorResponse = await aggregator.CreateTransaction(aggregatorRequest);
                    mapper.Map(aggregatorResponse, transaction);

                    if (!aggregatorResponse.Success)
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByAggregator, rejectionMessage: aggregatorResponse.ErrorMessage, rejectionReason: aggregatorResponse.RejectReasonCode);

                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.RejectedByAggregator}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, aggregatorResponse.Errors));
                    }
                    else
                    {
                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByAggregator);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Aggregator Create Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToConfirmByAggregator, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                    return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToConfirmByAggregator.ToString(), (ex as IntegrationException)?.Message));
                }
            }

            BadRequestObjectResult processorFailedRsponse = null;

            // create transaction in processor (Shva)
            try
            {
                mapper.Map(transaction, processorRequest);

                if (!pinpadDeal)
                {
                    if (token != null)
                    {
                        mapper.Map(token, processorRequest.CreditCardToken);
                        mapper.Map(dbToken.ShvaInitialTransactionDetails, processorRequest.InitialDeal, typeof(ShvaInitialTransactionDetails), typeof(Shva.Models.InitDealResultModel)); // TODO: remove direct Shva reference
                    }
                    else
                    {
                        mapper.Map(model.CreditCardSecureDetails, processorRequest.CreditCardToken);
                    }
                }

                processorRequest.ProcessorSettings = processorSettings;

                if (pinpadDeal)
                {
                    //2. Device is processing transaction
                    await NotifyStatusChanged(transaction, Messages.DeviceIsProcessingTransaction);
                }

                var processorResponse = pinpadDeal ? await pinpadProcessor.CreateTransaction(processorRequest) : await processor.CreateTransaction(processorRequest);

                mapper.Map(processorResponse, transaction);

                if (!processorResponse.Success)
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedByProcessor, TransactionFinalizationStatusEnum.Initial, rejectionMessage: processorResponse.ErrorMessage, rejectionReason: processorResponse.RejectReasonCode);

                    if (processorResponse.RejectReasonCode == RejectionReasonEnum.AuthorizationCodeRequired)
                    {
                        var message = Messages.AuthorizationCodeRequired.Replace("@number", processorResponse.TelToGetAuthNum).Replace("@retailer", processorResponse.CompRetailerNum);
                        processorFailedRsponse = BadRequest(new OperationResponse(message, StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier) { AdditionalData = JObject.FromObject(new { authorizationCodeRequired = true, message }) } );
                    }
                    else
                    {
                        processorFailedRsponse = BadRequest(new OperationResponse($"{Transactions.Shared.Messages.RejectedByProcessor}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, processorResponse.Errors));
                    }
                }
                else
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.ConfirmedByProcessor);

                    if (model.InitialJ5TransactionID != null)
                    {
                        var transactionJ5 = EnsureExists(await transactionsService.GetTransaction(t => t.PaymentTransactionID == model.InitialJ5TransactionID));
                        await transactionsService.UpdateEntityWithStatus(transactionJ5, TransactionStatusEnum.Completed);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Processor Create Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToConfirmByProcesor, TransactionFinalizationStatusEnum.Initial, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                processorFailedRsponse = BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToConfirmByProcesor.ToString(), (ex as IntegrationException)?.Message));
            }

            if (aggregator.ShouldBeProcessedByAggregator(transaction.TransactionType, transaction.SpecialTransactionType, transaction.JDealType))
            {
                // reject to clearing house in case of shva error
                if (processorFailedRsponse != null)
                {
                    try
                    {
                        var aggregatorRequest = mapper.Map<AggregatorCancelTransactionRequest>(transaction);
                        aggregatorRequest.AggregatorSettings = aggregatorSettings;
                        aggregatorRequest.RejectionReason = TransactionStatusEnum.FailedToConfirmByProcesor.ToString();

                        var aggregatorResponse = await aggregator.CancelTransaction(aggregatorRequest);
                        mapper.Map(aggregatorResponse, transaction);

                        if (!aggregatorResponse.Success)
                        {
                            logger.LogError($"Aggregator Cancel Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                            await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                            return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}: {aggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier));
                        }
                        else
                        {
                            await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.CanceledByAggregator);

                            return processorFailedRsponse;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Aggregator Cancel Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                        await transactionsService.UpdateEntityWithStatus(transaction, finalizationStatus: TransactionFinalizationStatusEnum.FailedToCancelByAggregator);

                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionFinalizationStatusEnum.FailedToCancelByAggregator.ToString(), (ex as IntegrationException)?.Message));
                    }
                }

                // commit transaction in aggregator (Clearing House or upay)
                else
                {
                    try
                    {
                        var commitAggregatorRequest = mapper.Map<AggregatorCommitTransactionRequest>(transaction);

                        commitAggregatorRequest.AggregatorSettings = aggregatorSettings;

                        var commitAggregatorResponse = await aggregator.CommitTransaction(commitAggregatorRequest);
                        mapper.Map(commitAggregatorResponse, transaction);

                        if (!commitAggregatorResponse.Success)
                        {
                            // NOTE: In case of failed commit, transaction should not be transmitted to Shva

                            logger.LogError($"Aggregator Commit Transaction request error. TransactionID: {transaction.PaymentTransactionID}");

                            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionMessage: commitAggregatorResponse.ErrorMessage, rejectionReason: commitAggregatorResponse.RejectReasonCode);

                            return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}: {commitAggregatorResponse.ErrorMessage}", StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, commitAggregatorResponse.Errors));
                        }
                        else
                        {
                            await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.AwaitingForTransmission, transactionOperationCode: TransactionOperationCodesEnum.CommitedByAggregator);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Aggregator Commit Transaction request failed. TransactionID: {transaction.PaymentTransactionID}");

                        await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.FailedToCommitByAggregator, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                        return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.FailedToProcessTransaction}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToCommitByAggregator.ToString(), (ex as IntegrationException)?.Message));
                    }
                }
            }
            else if (processorFailedRsponse != null)
            {
                return processorFailedRsponse;
            }
            else
            {
                if (jDealType == JDealTypeEnum.J5)
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.AwaitingForSelectJ5);
                }
                else if (jDealType != JDealTypeEnum.J4)
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.Completed);
                }
                else
                {
                    //If aggregator is not required transaction is eligible for transmission
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.AwaitingForTransmission);
                }
            }

            if (billingDeal != null && jDealType == JDealTypeEnum.J4)
            {
                billingDeal.UpdateNextScheduledDate(transaction.TransactionTimestamp, transaction.TransactionDate);

                await billingDealService.UpdateEntity(billingDeal);
            }

            var endResponse = new OperationResponse(Transactions.Shared.Messages.TransactionCreated, StatusEnum.Success, transaction.PaymentTransactionID);

            if (jDealType == JDealTypeEnum.J5)
            {
                endResponse.InnerResponse = new OperationResponse(string.Format(Transactions.Shared.Messages.J5ExpirationDate, transaction.TransactionTimestamp.Value.AddDays(terminal.Settings.J5ExpirationDays)), StatusEnum.Success);
            }

            // TODO: validate InvoiceDetails
            if (model.IssueInvoice == true && model.Currency == CurrencyEnum.ILS)
            {
                if (!string.IsNullOrWhiteSpace(transaction.DealDetails.ConsumerEmail) && !string.IsNullOrWhiteSpace(transaction.DealDetails.ConsumerName))
                {
                    using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
                    {
                        try
                        {
                            Invoice invoiceRequest = new Invoice();
                            mapper.Map(transaction, invoiceRequest);
                            invoiceRequest.InvoiceDetails = model.InvoiceDetails;

                            var creditCardDetails = invoiceRequest.PaymentDetails.FirstOrDefault(d => d.PaymentType == SharedIntegration.Models.PaymentTypeEnum.Card) as SharedIntegration.Models.PaymentDetails.CreditCardPaymentDetails;

                            // in case if consumer name/natid is not specified in deal details, get it from credit card details
                            invoiceRequest.DealDetails.UpdateDealDetails(creditCardDetails);

                            invoiceRequest.MerchantID = terminal.MerchantID;

                            invoiceRequest.ApplyAuditInfo(httpContextAccessor);

                            invoiceRequest.Calculate();

                            await invoiceService.CreateEntity(invoiceRequest, dbTransaction: dbTransaction);

                            endResponse.InnerResponse = new OperationResponse(Transactions.Shared.Messages.InvoiceCreated, StatusEnum.Success, invoiceRequest.InvoiceID);

                            transaction.InvoiceID = invoiceRequest.InvoiceID;

                            await transactionsService.UpdateEntity(transaction, Transactions.Shared.Messages.InvoiceCreated, TransactionOperationCodesEnum.InvoiceCreated, dbTransaction: dbTransaction);

                            var invoicesToResend = await invoiceService.StartSending(terminal.TerminalID, new Guid[] { invoiceRequest.InvoiceID }, dbTransaction);

                            // TODO: validate, rollback
                            if (invoicesToResend.Count() > 0)
                            {
                                await invoiceQueue.PushToQueue(invoicesToResend.First());
                            }

                            await dbTransaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Failed to create invoice. TransactionID: {transaction.PaymentTransactionID}");

                            endResponse.InnerResponse = new OperationResponse($"{Transactions.Shared.Messages.FailedToCreateInvoice}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, "FailedToCreateInvoice", ex.Message);

                            await dbTransaction.RollbackAsync();
                        }
                    }
                }
                else
                {
                    endResponse.InnerResponse = new OperationResponse($"{Transactions.Shared.Messages.FailedToCreateInvoice} - {Transactions.Shared.Messages.ConsumerNameAndConsumerEmailRequired}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, "FailedToCreateInvoice", Transactions.Shared.Messages.ConsumerNameAndConsumerEmailRequired);
                }
            }

            try
            {
                await SendTransactionSuccessEmails(transaction, terminal);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"{nameof(ProcessTransaction)}: EmailSend");
            }

            return CreatedAtAction(nameof(GetTransaction), new { transactionID = transaction.PaymentTransactionID }, endResponse);
        }

        private async Task<ActionResult<OperationResponse>> ProcessBankTransaction(CreateTransactionRequest model, SpecialTransactionTypeEnum specialTransactionType = SpecialTransactionTypeEnum.RegularDeal, BillingDeal billingDeal = null)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            //TODO: validate bank parameters
            //TransactionTerminalSettingsValidator.Validate(terminal.Settings, model, null, null, specialTransactionType);

            var transaction = mapper.Map<PaymentTransaction>(model);

            // NOTE: this is security assignment
            mapper.Map(terminal, transaction);

            bool pinpadDeal = model.PinPad ?? false;

            transaction.SpecialTransactionType = specialTransactionType;
            transaction.BillingDealID = billingDeal?.BillingDealID;
            transaction.DocumentOrigin = GetDocumentOrigin(billingDeal?.BillingDealID, null, false);

            // ensuring that bank unrelated fields are null
            transaction.CreditCardDetails = null;
            transaction.CreditCardToken = null;
            transaction.ShvaTransactionDetails = null;

            if (transaction.DealDetails == null)
            {
                transaction.DealDetails = new Business.Entities.DealDetails();
            }

            if (model.IssueInvoice == true && model.InvoiceDetails == null)
            {
                //TODO: Handle refund & credit default invoice types
                model.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
            }

            // Check consumer
            var consumer = transaction.DealDetails.ConsumerID != null ?
                EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == transaction.DealDetails.ConsumerID && d.TerminalID == terminal.TerminalID), "Consumer") : null;

            // Update details if needed
            transaction.DealDetails.UpdateDealDetails(consumer, terminal.Settings, transaction, null);
            if (model.IssueInvoice.GetValueOrDefault())
            {
                model.InvoiceDetails.UpdateInvoiceDetails(terminal.InvoiceSettings, transaction);
            }

            transaction.Calculate();

            transaction.MerchantIP = GetIP();
            transaction.CorrelationId = GetCorrelationID();

            await transactionsService.CreateEntity(transaction);
            metrics.TrackTransactionEvent(transaction, TransactionOperationCodesEnum.TransactionCreated);

            if (billingDeal != null)
            {
                billingDeal.UpdateNextScheduledDate(transaction.TransactionTimestamp, transaction.TransactionDate);

                await billingDealService.UpdateEntity(billingDeal);
            }

            var endResponse = new OperationResponse(Transactions.Shared.Messages.TransactionCreated, StatusEnum.Success, transaction.PaymentTransactionID);

            // TODO: validate InvoiceDetails
            if (model.IssueInvoice == true && model.Currency == CurrencyEnum.ILS)
            {
                if (!string.IsNullOrWhiteSpace(transaction.DealDetails.ConsumerEmail) && !string.IsNullOrWhiteSpace(transaction.DealDetails.ConsumerName))
                {
                    using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
                    {
                        try
                        {
                            Invoice invoiceRequest = new Invoice();
                            mapper.Map(transaction, invoiceRequest);
                            invoiceRequest.InvoiceDetails = model.InvoiceDetails;

                            invoiceRequest.MerchantID = terminal.MerchantID;

                            invoiceRequest.ApplyAuditInfo(httpContextAccessor);

                            invoiceRequest.Calculate();

                            await invoiceService.CreateEntity(invoiceRequest, dbTransaction: dbTransaction);

                            endResponse.InnerResponse = new OperationResponse(Transactions.Shared.Messages.InvoiceCreated, StatusEnum.Success, invoiceRequest.InvoiceID);

                            transaction.InvoiceID = invoiceRequest.InvoiceID;

                            await transactionsService.UpdateEntity(transaction, Transactions.Shared.Messages.InvoiceCreated, TransactionOperationCodesEnum.InvoiceCreated, dbTransaction: dbTransaction);

                            var invoicesToResend = await invoiceService.StartSending(terminal.TerminalID, new Guid[] { invoiceRequest.InvoiceID }, dbTransaction);

                            // TODO: validate, rollback
                            if (invoicesToResend.Count() > 0)
                            {
                                await invoiceQueue.PushToQueue(invoicesToResend.First());
                            }

                            await dbTransaction.CommitAsync();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, $"Failed to create invoice. TransactionID: {transaction.PaymentTransactionID}");

                            endResponse.InnerResponse = new OperationResponse($"{Transactions.Shared.Messages.FailedToCreateInvoice}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, "FailedToCreateInvoice", ex.Message);

                            await dbTransaction.RollbackAsync();
                        }
                    }
                }
                else
                {
                    endResponse.InnerResponse = new OperationResponse($"{Transactions.Shared.Messages.FailedToCreateInvoice}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, "FailedToCreateInvoice", "Consumer Name and Consumer Email are required");
                }
            }

            try
            {
                await SendTransactionSuccessEmails(transaction, terminal);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"{nameof(ProcessTransaction)}: Email sending failed");
            }

            return CreatedAtAction(nameof(GetTransaction), new { transactionID = transaction.PaymentTransactionID }, endResponse);
        }

        private async Task<ActionResult<OperationResponse>> ProcessBillingInvoice(BillingDeal model)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            if (model.Currency != CurrencyEnum.ILS)
            {
                return new OperationResponse($"{Transactions.Shared.Messages.FailedToCreateInvoice}", model.BillingDealID,
                    httpContextAccessor.TraceIdentifier, "FailedToCreateInvoice", "Only ILS invoices allowed");
            }

            if (string.IsNullOrWhiteSpace(model.DealDetails.ConsumerEmail) || string.IsNullOrWhiteSpace(model.DealDetails.ConsumerName))
            {
                return new OperationResponse($"{Transactions.Shared.Messages.FailedToCreateInvoice}", model.BillingDealID,
                    httpContextAccessor.TraceIdentifier, "FailedToCreateInvoice", "Both ConsumerEmail and ConsumerName must be specified");
            }

            Invoice invoiceRequest = new Invoice();
            mapper.Map(model, invoiceRequest);

            invoiceRequest.DocumentOrigin = GetDocumentOrigin(model?.BillingDealID, null, false);

            if (invoiceRequest.DealDetails == null)
            {
                invoiceRequest.DealDetails = new Business.Entities.DealDetails();
            }

            if (model.InvoiceDetails == null)
            {
                //TODO: Handle refund & credit default invoice types
                model.InvoiceDetails = new SharedIntegration.Models.Invoicing.InvoiceDetails { InvoiceType = terminal.InvoiceSettings.DefaultInvoiceType.GetValueOrDefault() };
            }

            // Check consumer
            var consumer = model.DealDetails.ConsumerID != null ?
                EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == model.DealDetails.ConsumerID && d.TerminalID == terminal.TerminalID), "Consumer") : null;

            // Update details if needed
            invoiceRequest.DealDetails.UpdateDealDetails(consumer, terminal.Settings, invoiceRequest, null);

            model.InvoiceDetails.UpdateInvoiceDetails(terminal.InvoiceSettings, null);

            invoiceRequest.Calculate();

            //invoiceRequest.MerchantIP = GetIP();
            invoiceRequest.CorrelationId = GetCorrelationID();

            model.UpdateNextScheduledDate(invoiceRequest.InvoiceTimestamp, invoiceRequest.InvoiceDate);

            await billingDealService.UpdateEntity(model);

            OperationResponse endResponse = null;
            Guid? invoiceID = null;

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                try
                {
                    invoiceRequest.InvoiceDetails = model.InvoiceDetails;

                    invoiceRequest.MerchantID = terminal.MerchantID;

                    invoiceRequest.ApplyAuditInfo(httpContextAccessor);

                    invoiceRequest.Calculate();

                    await invoiceService.CreateEntity(invoiceRequest, dbTransaction: dbTransaction);

                    invoiceID = invoiceRequest.InvoiceID;

                    endResponse = new OperationResponse(Transactions.Shared.Messages.InvoiceCreated, StatusEnum.Success, invoiceID);

                    //await transactionsService.UpdateEntity(transaction, Transactions.Shared.Messages.InvoiceCreated, TransactionOperationCodesEnum.InvoiceCreated, dbTransaction: dbTransaction);

                    var invoicesToResend = await invoiceService.StartSending(terminal.TerminalID, new Guid[] { invoiceRequest.InvoiceID }, dbTransaction);

                    // TODO: validate, rollback
                    if (invoicesToResend.Count() > 0)
                    {
                        await invoiceQueue.PushToQueue(invoicesToResend.First());
                    }

                    await dbTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to create invoice. BillingDealID: {model.BillingDealID}");

                    endResponse = new OperationResponse($"{Transactions.Shared.Messages.FailedToCreateInvoice}", model.BillingDealID, httpContextAccessor.TraceIdentifier, "FailedToCreateInvoice", ex.Message);

                    await dbTransaction.RollbackAsync();
                }
            }

            try
            {
                await invoicingController.SendInvoiceEmail(invoiceRequest.DealDetails.ConsumerEmail, invoiceRequest, terminal);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"{nameof(ProcessTransaction)}: Email sending failed");
            }

            return CreatedAtAction(nameof(GetTransaction), new { billingDealID = model.BillingDealID, invoiceID }, endResponse);
        }

        private async Task<ShvaTransactionDetails> GetLastShvaTransactionDetails(string shvaTerminalNumber)
        {
            return await this.transactionsService.GetTransactions().Where(x => x.ShvaTransactionDetails.ShvaTerminalID == shvaTerminalNumber)
                .OrderByDescending(d => d.TransactionDate).Select(d => d.ShvaTransactionDetails).FirstOrDefaultAsync();
        }

        private DocumentOriginEnum GetDocumentOrigin(Guid? billingDealID, Guid? paymentRequestID, bool pinpad)
        {
            if (pinpad)
            {
                return DocumentOriginEnum.Device;
            }
            else if (billingDealID.HasValue)
            {
                return DocumentOriginEnum.Billing;
            }
            else if (User.IsTerminal())
            {
                return DocumentOriginEnum.API;
            }
            else if (User.IsMerchant())
            {
                return DocumentOriginEnum.UI;
            }
            else if (paymentRequestID.HasValue)
            {
                return DocumentOriginEnum.PaymentRequest;
            }
            else if (User.IsAdmin())
            {
                // TODO: checkout identty
                return DocumentOriginEnum.Checkout;
            }
            else
            {
                // TODO: how to determine device
                return DocumentOriginEnum.Device;
            }
        }

        private async Task SendTransactionSuccessEmails(PaymentTransaction transaction, Terminal terminal, string emailTo = null)
        {
            if (transaction.DealDetails?.ConsumerEmail == null)
            {
                return;

                //throw new ArgumentNullException(nameof(transaction.DealDetails.ConsumerEmail));
            }

            var settings = terminal.PaymentRequestSettings;

            var emailSubject = "Payment Success";
            var emailTemplateCode = nameof(PaymentTransaction);
            var substitutions = new List<TextSubstitution>
            {
                new TextSubstitution(nameof(settings.MerchantLogo), string.IsNullOrWhiteSpace(settings.MerchantLogo) ? $"{apiSettings.CheckoutPortalUrl}/img/merchant-logo.png" : $"{apiSettings.BlobBaseAddress}/{settings.MerchantLogo}"),
                new TextSubstitution(nameof(terminal.Merchant.MarketingName), terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName),
                new TextSubstitution(nameof(transaction.TransactionDate), TimeZoneInfo.ConvertTimeFromUtc(transaction.TransactionDate.GetValueOrDefault(), UserCultureInfo.TimeZone).ToString("d")), // TODO: locale
                new TextSubstitution(nameof(transaction.TransactionAmount), $"{transaction.TotalAmount.ToString("F2")}{transaction.Currency.GetCurrencySymbol()}"),

                new TextSubstitution(nameof(transaction.ShvaTransactionDetails.ShvaTerminalID), transaction.ShvaTransactionDetails?.ShvaTerminalID ?? string.Empty),
                new TextSubstitution(nameof(transaction.ShvaTransactionDetails.ShvaShovarNumber), transaction.ShvaTransactionDetails?.ShvaShovarNumber ?? string.Empty),

                new TextSubstitution(nameof(transaction.CreditCardDetails.CardNumber), transaction.CreditCardDetails?.CardNumber ?? string.Empty),
                new TextSubstitution(nameof(transaction.CreditCardDetails.CardOwnerName), transaction.CreditCardDetails?.CardOwnerName ?? string.Empty),

                new TextSubstitution(nameof(transaction.DealDetails.DealDescription), transaction.DealDetails?.DealDescription ?? string.Empty)
            };

            var dictionaries = DictionariesService.GetDictionaries(CurrentCulture);

            var transactionTypeKey = typeof(TransactionTypeEnum).GetDataContractAttrForEnum(transaction.TransactionType.ToString());
            var cardPresenceKey = typeof(CardPresenceEnum).GetDataContractAttrForEnum(transaction.CardPresence.ToString());
            var originKey = typeof(DocumentOriginEnum).GetDataContractAttrForEnum(transaction.DocumentOrigin.ToString());

            var transactionTypeStr = dictionaries.TransactionTypeEnum[transactionTypeKey];
            var cardPresenceTypeStr = dictionaries.CardPresenceEnum[cardPresenceKey];
            var originStr = dictionaries.DocumentOriginEnum[originKey];

            substitutions.Add(new TextSubstitution(nameof(transaction.TransactionType), transactionTypeStr));
            substitutions.Add(new TextSubstitution(nameof(transaction.CardPresence), cardPresenceTypeStr));
            substitutions.Add(new TextSubstitution(nameof(transaction.DocumentOrigin), originStr));

            var email = new Email
            {
                EmailTo = transaction.DealDetails.ConsumerEmail,
                Subject = emailSubject,
                TemplateCode = emailTemplateCode,
                Substitutions = substitutions.ToArray()
            };

            await emailSender.SendEmail(email);

            if (terminal.Settings.SendTransactionSlipEmailToMerchant == true && terminal.BillingSettings.BillingNotificationsEmails?.Count() > 0)
            {
                var merchantEmailTemplateCode = nameof(PaymentTransaction) + "Merchant";

                foreach (var notificationEmail in terminal.BillingSettings.BillingNotificationsEmails)
                {
                    var emailToMerchant = new Email
                    {
                        EmailTo = notificationEmail,
                        Subject = emailSubject,
                        TemplateCode = merchantEmailTemplateCode,
                        Substitutions = substitutions.ToArray()
                    };

                    await emailSender.SendEmail(emailToMerchant);
                }
            }
        }

        private async Task SendBillingInvoiceSuccessEmails(BillingDeal billingDeal, Invoice invoice, Terminal terminal, string emailTo = null)
        {
            if (billingDeal.DealDetails?.ConsumerEmail == null)
            {
                return;

                //throw new ArgumentNullException(nameof(transaction.DealDetails.ConsumerEmail));
            }

            var settings = terminal.PaymentRequestSettings;

            var emailSubject = "Invoice";
            var emailTemplateCode = nameof(PaymentTransaction);
            var substitutions = new List<TextSubstitution>
            {
                new TextSubstitution(nameof(settings.MerchantLogo), string.IsNullOrWhiteSpace(settings.MerchantLogo) ? $"{apiSettings.CheckoutPortalUrl}/img/merchant-logo.png" : $"{apiSettings.BlobBaseAddress}/{settings.MerchantLogo}"),
                new TextSubstitution(nameof(terminal.Merchant.MarketingName), terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName),
                new TextSubstitution(nameof(PaymentTransaction.TransactionDate), TimeZoneInfo.ConvertTimeFromUtc(billingDeal.CurrentTransactionTimestamp.GetValueOrDefault(), UserCultureInfo.TimeZone).ToString("d")), // TODO: locale
                new TextSubstitution(nameof(billingDeal.TransactionAmount), $"{billingDeal.TransactionAmount.ToString("F2")}{billingDeal.Currency.GetCurrencySymbol()}"),

                new TextSubstitution(nameof(PaymentTransaction.ShvaTransactionDetails.ShvaTerminalID), string.Empty),
                new TextSubstitution(nameof(PaymentTransaction.ShvaTransactionDetails.ShvaShovarNumber), string.Empty),

                new TextSubstitution(nameof(billingDeal.CreditCardDetails.CardNumber), billingDeal.CreditCardDetails?.CardNumber ?? string.Empty),
                new TextSubstitution(nameof(billingDeal.CreditCardDetails.CardOwnerName), billingDeal.CreditCardDetails?.CardOwnerName ?? string.Empty),

                new TextSubstitution(nameof(billingDeal.DealDetails.DealDescription), billingDeal.DealDetails?.DealDescription ?? string.Empty)
            };

            var dictionaries = DictionariesService.GetDictionaries(CurrentCulture);

            var originKey = typeof(DocumentOriginEnum).GetDataContractAttrForEnum(billingDeal.DocumentOrigin.ToString());

            var originStr = dictionaries.DocumentOriginEnum[originKey];

            substitutions.Add(new TextSubstitution(nameof(PaymentTransaction.DocumentOrigin), originStr));

            var email = new Email
            {
                EmailTo = billingDeal.DealDetails.ConsumerEmail,
                Subject = emailSubject,
                TemplateCode = emailTemplateCode,
                Substitutions = substitutions.ToArray()
            };

            await emailSender.SendEmail(email);

            if (terminal.Settings.SendTransactionSlipEmailToMerchant == true && terminal.BillingSettings.BillingNotificationsEmails?.Count() > 0)
            {
                var merchantEmailTemplateCode = nameof(PaymentTransaction) + "Merchant";

                foreach (var notificationEmail in terminal.BillingSettings.BillingNotificationsEmails)
                {
                    var emailToMerchant = new Email
                    {
                        EmailTo = notificationEmail,
                        Subject = emailSubject,
                        TemplateCode = merchantEmailTemplateCode,
                        Substitutions = substitutions.ToArray()
                    };

                    await emailSender.SendEmail(emailToMerchant);
                }
            }
        }

        private async Task<OperationResponse> NextBillingDeal(BillingDeal billingDeal)
        {
            var transaction = mapper.Map<CreateTransactionRequest>(billingDeal);

            transaction.CardPresence = CardPresenceEnum.CardNotPresent;
            transaction.TransactionType = TransactionTypeEnum.RegularDeal;

            try
            {
                ActionResult<OperationResponse> actionResult = null;

                if (billingDeal.InvoiceOnly)
                {
                    actionResult = await ProcessBillingInvoice(billingDeal);
                }
                else if (billingDeal.BankDetails != null)
                {
                    actionResult = await ProcessBankTransaction(transaction, specialTransactionType: SpecialTransactionTypeEnum.RegularDeal, billingDeal: billingDeal);
                }
                else
                {
                    var token = EnsureExists(await keyValueStorage.Get(billingDeal.CreditCardToken.ToString()), "CreditCardToken");
                    actionResult = await ProcessTransaction(transaction, token, specialTransactionType: SpecialTransactionTypeEnum.RegularDeal, initialTransactionID: billingDeal.InitialTransactionID, billingDeal: billingDeal);
                }

                var response = actionResult.Result as ObjectResult;
                return response.Value as OperationResponse;
            }
            catch (BusinessException businessEx)
            {
                logger.LogError($"{nameof(NextBillingDeal)}: {billingDeal.BillingDealID}, Error: {string.Join("; ", businessEx.Errors?.Select(b => $"{b.Code}:{b.Description}"))}");
                return new OperationResponse { Message = businessEx.Message, Status = StatusEnum.Error, Errors = businessEx.Errors };
            }
            catch (EntityConflictException businessEx)
            {
                logger.LogError($"{nameof(NextBillingDeal)}: {billingDeal.BillingDealID}, Error: {businessEx.Message}: {businessEx.EntityType} {businessEx.ConflictDetails}");
                return new OperationResponse { Message = $"{businessEx.Message}: {businessEx.EntityType} {businessEx.ConflictDetails}", Status = StatusEnum.Error, Errors = new List<Error> { new Error(businessEx.EntityType, businessEx.ConflictDetails) } };
            }
            catch (Exception ex)
            {
                logger.LogError($"{nameof(NextBillingDeal)}: {billingDeal.BillingDealID}, Error: {ex.Message}");
                return new OperationResponse { Message = $"System error", Status = StatusEnum.Error };
            }
        }

        /// <summary>
        /// Notify SignalR (if needed) about transaction status change
        /// </summary>
        /// <param name="transaction">Transaction data</param>
        /// <param name="message">Message to display</param>
        private async Task NotifyStatusChanged(PaymentTransaction transaction, string message)
        {
            if (transaction.ConnectionID == null)
            {
                return;
            }

            var payload = new Shared.Models.TransactionStatusChangedHubModel
            {
                PaymentTransactionID = transaction.PaymentTransactionID,
                Status = transaction.Status,
                StatusString = message
            };

            try
            {
                await transactionsHubContext.Clients.Clients(transaction.ConnectionID).TransactionStatusChanged(payload);
            }
            catch (Exception ex)
            {
                logger.LogError($"{nameof(NotifyStatusChanged)} ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates consumer based on transaction data. To be used when "SaveCreditCard" flag is true, but no customer is supplied
        /// </summary>
        /// <param name="transaction">Transaction</param>
        /// <param name="merchantID">Merchant ID</param>
        /// <returns>New customer ID</returns>
        private async Task<Guid?> CreateConsumer(CreateTransactionRequest transaction, Guid merchantID)
        {
            var consumer = new Merchants.Business.Entities.Billing.Consumer();

            mapper.Map(transaction.DealDetails, consumer);
            consumer.ConsumerName = (transaction.CardOwnerName ?? transaction.CreditCardSecureDetails?.CardOwnerName) ?? transaction.DealDetails?.ConsumerName;
            consumer.ConsumerEmail = transaction.DealDetails?.ConsumerEmail;
            consumer.ConsumerNationalID = transaction.CardOwnerNationalID ?? transaction.CreditCardSecureDetails?.CardOwnerNationalID;
            consumer.TerminalID = transaction.TerminalID;
            consumer.MerchantID = merchantID;
            consumer.ApplyAuditInfo(httpContextAccessor);

            if (!(!string.IsNullOrWhiteSpace(consumer.ConsumerName) && !string.IsNullOrWhiteSpace(consumer.ConsumerEmail)))
            {
                return null;
            }

            await consumersService.CreateEntity(consumer);

            return consumer.ConsumerID;
        }
    }
}
