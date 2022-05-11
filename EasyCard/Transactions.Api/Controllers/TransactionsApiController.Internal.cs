using Ecwid.Models;
using Merchants.Business.Entities.Terminal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Email;
using Shared.Helpers.Events;
using Shared.Helpers.Security;
using Shared.Helpers.Templating;
using Shared.Integration;
using Shared.Integration.Exceptions;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Extensions;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Api.Validation;
using Transactions.Business.Entities;
using Transactions.Shared;
using Transactions.Shared.Enums;
using Z.EntityFramework.Plus;
using SharedBusiness = Shared.Business;
using SharedIntegration = Shared.Integration;

namespace Transactions.Api.Controllers
{
    public partial class TransactionsApiController
    {
        private async Task<Terminal> GetTerminal(Guid terminalID)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            // TODO: caching
            var systemSettings = await systemSettingsService.GetSystemSettings();

            // merge system settings with terminal settings
            mapper.Map(systemSettings, terminal);

            return terminal;
        }

        private async Task<ActionResult<OperationResponse>> ProcessTransaction(Terminal terminal, CreateTransactionRequest model, CreditCardTokenKeyVault token, JDealTypeEnum jDealType = JDealTypeEnum.J4, SpecialTransactionTypeEnum specialTransactionType = SpecialTransactionTypeEnum.RegularDeal, Guid? initialTransactionID = null, BillingDeal billingDeal = null, Guid? paymentRequestID = null, Func<IDbContextTransaction, Task> initialTransactionProcess = null, Func<IDbContextTransaction, Task> compensationTransactionProcess = null)
        {
            TransactionTerminalSettingsValidator.ValidatePinpad(terminal, model);

            TransactionTerminalSettingsValidator.Validate(terminal.Settings, model, token, jDealType, specialTransactionType, initialTransactionID);

            //TODO: map terminal to model?
            if (model.VATRate == null)
            {
                model.VATRate = terminal.Settings.VATRate;
            }

            var transaction = mapper.Map<PaymentTransaction>(model);

            if (jDealType == JDealTypeEnum.J5)
            {
                transaction.TransactionJ5ExpiredDate = DateTime.Now.AddDays(terminal.Settings.J5ExpirationDays);
            }

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

            // Update card information based on token
            CreditCardTokenDetails dbToken = null;

            if (token != null)
            {
                if (token.TerminalID != terminal.TerminalID)
                {
                    if (!(terminal.Settings.SharedCreditCardTokens == true))
                    {
                        throw new EntityNotFoundException(SharedBusiness.Messages.ApiMessages.EntityNotFound, "CreditCardToken", null);
                    }
                }

                if (token.CardExpiration?.Expired == true)
                {
                    return BadRequest(new OperationResponse($"{Messages.CreditCardExpired}", StatusEnum.Error, model.CreditCardToken));
                }

                mapper.Map(token, transaction.CreditCardDetails);
                mapper.Map(token, transaction);

                if (terminal.Settings.SharedCreditCardTokens == true)
                {
                    if (User.IsAdmin())
                    {
                        dbToken = await creditCardTokenService.GetTokensSharedAdmin(terminal.MerchantID, terminal.TerminalID).FirstOrDefaultAsync(d => d.CreditCardTokenID == model.CreditCardToken);
                    }
                    else
                    {
                        dbToken = await creditCardTokenService.GetTokensShared(terminal.TerminalID).FirstOrDefaultAsync(d => d.CreditCardTokenID == model.CreditCardToken);
                    }
                }
                else
                {
                    dbToken = await creditCardTokenService.GetTokens().FirstOrDefaultAsync(d => d.CreditCardTokenID == model.CreditCardToken);
                }

                if (transaction.InitialTransactionID == null)
                {
                    transaction.InitialTransactionID = dbToken?.InitialTransactionID;
                }

                if (transaction.DealDetails?.ConsumerID == null)
                {
                    transaction.DealDetails.ConsumerID = dbToken?.ConsumerID;
                }
            }
            else
            {
                mapper.Map(model.CreditCardSecureDetails, transaction.CreditCardDetails);
            }

            // Check consumer
            var consumer = transaction.DealDetails.ConsumerID != null ? EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == transaction.DealDetails.ConsumerID), "Consumer") : null;

            transaction.DealDetails.CheckConsumerDetails(consumer, dbToken);

            transaction.Calculate();

            // Update details if needed
            transaction.DealDetails.UpdateDealDetails(consumer, terminal.Settings, transaction, transaction.CreditCardDetails);

            transaction.CreditCardDetails.UpdateCreditCardDetails(consumer, model);

            // map consumer name from card details if needed
            transaction.DealDetails.UpdateDealDetails(transaction.CreditCardDetails);

            transaction.ApplyAuditInfo(httpContextAccessor);

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

            //TODO: if Bit parameters present, do not user resolver, use BitProcessor directly
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
            }

            if (initialTransactionProcess != null)
            {
                using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
                {
                    await initialTransactionProcess(dbTransaction);
                    await transactionsService.CreateEntity(transaction, dbTransaction);
                    await dbTransaction.CommitAsync();
                }
            }
            else
            {
                await transactionsService.CreateEntity(transaction);
            }

            var processorRequest = mapper.Map<ProcessorCreateTransactionRequest>(transaction);

            // TODO: move to automapper profile
            processorRequest.SapakMutavNo = terminal.Settings.RavMutavNumber;

            ActionResult<OperationResponse> failedRsponse =
             await ProcessTransactionAggregatorAndProcessor(model, token, transaction, pinpadDeal, dbToken, aggregator, processor, pinpadProcessor, aggregatorSettings, processorSettings, pinpadProcessorSettings, processorRequest, terminal);

            if (failedRsponse != null)
            {
                var resObj = failedRsponse.GetOperationResponse();

                _ = events.RaiseTransactionEvent(transaction, CustomEvent.TransactionRejected, $"{Transactions.Shared.Messages.FailedToProcessTransaction}: {resObj.Message}");

                if (compensationTransactionProcess != null)
                {
                    using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
                    {
                        await compensationTransactionProcess(dbTransaction);
                        await transactionsService.UpdateEntity(transaction, dbTransaction);
                        await dbTransaction.CommitAsync();
                    }
                }

                return failedRsponse;
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

            // TODO: move to event handler
            if (billingDeal != null && jDealType == JDealTypeEnum.J4)
            {
                billingDeal.UpdateNextScheduledDatAfterSuccess(transaction.PaymentTransactionID, transaction.TransactionTimestamp, transaction.TransactionDate);

                await billingDealService.UpdateEntityWithHistory(billingDeal, Messages.TransactionCreated, BillingDealOperationCodesEnum.TransactionCreated);
            }

            var endResponse = new OperationResponse(Transactions.Shared.Messages.TransactionCreated, StatusEnum.Success, transaction.PaymentTransactionID);

            if (jDealType == JDealTypeEnum.J5)
            {
                endResponse.InnerResponse = new OperationResponse(string.Format(Transactions.Shared.Messages.J5ExpirationDate, transaction.TransactionTimestamp.Value.AddDays(terminal.Settings.J5ExpirationDays)), StatusEnum.Success);
            }

            endResponse.InnerResponse = await invoicingController.ProcessInvoice(terminal, transaction, model.InvoiceDetails);

            // TODO: move to event handler
            await ProcessEcwid(transaction, endResponse);

            // TODO: move to event handler
            _ = SendTransactionSuccessEmails(transaction, terminal);

            _ = events.RaiseTransactionEvent(transaction, CustomEvent.TransactionCreated);

            return CreatedAtAction(nameof(GetTransaction), new { transactionID = transaction.PaymentTransactionID }, endResponse);
        }

        private async Task<ActionResult<OperationResponse>> ProcessTransactionAggregatorAndProcessor(CreateTransactionRequest model, CreditCardTokenKeyVault token, PaymentTransaction transaction, bool pinpadDeal, CreditCardTokenDetails dbToken, IAggregator aggregator, IProcessor processor, IProcessor pinpadProcessor, object aggregatorSettings, object processorSettings, object pinpadProcessorSettings, ProcessorCreateTransactionRequest processorRequest, Terminal terminal)
        {
            ProcessorPreCreateTransactionResponse pinpadPreCreateResult = null;

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
            else
            {
                try
                {
                    processorRequest.ThreeDSecure = await Process3dSecure(terminal, model, dbToken, transaction);
                }
                catch (Exception ex)
                {
                    await transactionsService.UpdateEntityWithStatus(transaction, TransactionStatusEnum.RejectedBy3Dsecure, rejectionReason: RejectionReasonEnum.Unknown, rejectionMessage: ex.Message);

                    return BadRequest(new OperationResponse($"{Transactions.Shared.Messages.RejectedBy3DSecure}", transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier, TransactionStatusEnum.FailedToConfirmByAggregator.ToString(), $"{transaction.ThreeDSServerTransID}"));
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

            ActionResult<OperationResponse> processorFailedRsponse = null;

            // create transaction in processor (Shva)
            try
            {
                mapper.Map(transaction, processorRequest);

                if (!pinpadDeal)
                {
                    if (token != null)
                    {
                        mapper.Map(token, processorRequest.CreditCardToken);

                        if (dbToken != null)
                        {
                            mapper.Map(dbToken.ShvaInitialTransactionDetails, processorRequest.InitialDeal, typeof(ShvaInitialTransactionDetails), typeof(Shva.Models.InitDealResultModel)); // TODO: remove direct Shva reference
                        }
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
                        processorFailedRsponse = BadRequest(new OperationResponse(message, StatusEnum.Error, transaction.PaymentTransactionID, httpContextAccessor.TraceIdentifier) { AdditionalData = JObject.FromObject(new { authorizationCodeRequired = true, message }) });
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

                            return null;
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
            else
            {
                return processorFailedRsponse;
            }
        }

        private async Task ProcessEcwid(PaymentTransaction transaction, OperationResponse endResponse)
        {
            if (transaction.Extension != null)
            {
                try
                {
                    var ecwidPayload = transaction.Extension.ToObject<EcwidTransactionExtension>();
                    if (ecwidPayload != null && ecwidPayload.Valid())
                    {
                        var ecwidResponse = await ecwidApiClient.UpdateOrderStatus(new Ecwid.Api.Models.EcwidUpdateOrderStatusRequest
                        {
                            PaymentTransactionID = transaction.PaymentTransactionID,
                            ReferenceTransactionID = ecwidPayload.ReferenceTransactionID,
                            StoreID = ecwidPayload.StoreID,
                            Status = endResponse.Status == StatusEnum.Success ?
                                    Ecwid.Api.Models.EcwidOrderStatusEnum.PAID : Ecwid.Api.Models.EcwidOrderStatusEnum.CANCELLED,
                            CorrelationId = transaction.CorrelationId,
                            Token = ecwidPayload.Token,
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Ecwid update order status failed: {ex.Message}. TransactionID: {transaction.PaymentTransactionID}");
                }
            }
        }

        private async Task<ThreeDSIntermediateData> Process3dSecure(Terminal terminal, CreateTransactionRequest model, CreditCardTokenDetails token, PaymentTransaction transaction)
        {
            if (terminal.Support3DSecure == true && !(model.PinPad == true) && token == null)
            {
                if (!string.IsNullOrWhiteSpace(transaction.ThreeDSServerTransID))
                {
                    var threeDSecure = await threeDSIntermediateStorage.GetIntermediateData(transaction.ThreeDSServerTransID);
                    transaction.ThreeDSChallengeID = threeDSecure?.ThreeDSChallengeID;

                    // TODO: add converter
                    if (threeDSecure?.TransStatus != "Y")
                    {
                         throw new BusinessException($"{Transactions.Shared.Messages.RejectedBy3DSecure}: {transaction.ThreeDSServerTransID}");
                    }
                    else
                    {
                        return threeDSecure;
                    }
                }
            }

            return null;
        }

        // TODO: extract controller
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
                EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.ConsumerID == transaction.DealDetails.ConsumerID), "Consumer") : null;

            // Update details if needed
            transaction.DealDetails.UpdateDealDetails(consumer, terminal.Settings, transaction, null);

            transaction.Calculate();

            transaction.ApplyAuditInfo(httpContextAccessor);

            await transactionsService.CreateEntity(transaction);

            if (billingDeal != null)
            {
                billingDeal.UpdateNextScheduledDatAfterSuccess(transaction.PaymentTransactionID, transaction.TransactionTimestamp, transaction.TransactionDate);

                await billingDealService.UpdateEntity(billingDeal);
            }

            var endResponse = new OperationResponse(Transactions.Shared.Messages.TransactionCreated, StatusEnum.Success, transaction.PaymentTransactionID);

            endResponse.InnerResponse = await invoicingController.ProcessInvoice(terminal, transaction, model.InvoiceDetails);

            _ = SendTransactionSuccessEmails(transaction, terminal);

            return CreatedAtAction(nameof(GetTransaction), new { transactionID = transaction.PaymentTransactionID }, endResponse);
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
            try
            {
                if (terminal.Settings.SendTransactionSlipEmailToConsumer != true && terminal.Settings.SendTransactionSlipEmailToMerchant != true)
                {
                    return;
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
                    new TextSubstitution(nameof(transaction.NumberOfPayments), transaction.NumberOfPayments.ToString()),
                    new TextSubstitution(nameof(transaction.InitialPaymentAmount), $"{transaction.InitialPaymentAmount.ToString("F2")}{transaction.Currency.GetCurrencySymbol()}"),
                    new TextSubstitution(nameof(transaction.InstallmentPaymentAmount), $"{transaction.InstallmentPaymentAmount.ToString("F2")}{transaction.Currency.GetCurrencySymbol()}"),

                    new TextSubstitution(nameof(transaction.ShvaTransactionDetails.ShvaTerminalID), transaction.ShvaTransactionDetails?.ShvaTerminalID ?? string.Empty),
                    new TextSubstitution(nameof(transaction.ShvaTransactionDetails.ShvaShovarNumber), transaction.ShvaTransactionDetails?.ShvaShovarNumber ?? string.Empty),

                    new TextSubstitution(nameof(transaction.CreditCardDetails.CardNumber), transaction.CreditCardDetails?.CardNumber ?? string.Empty),
                    new TextSubstitution(nameof(transaction.CreditCardDetails.CardOwnerName), transaction.CreditCardDetails?.CardOwnerName ?? string.Empty),

                    new TextSubstitution(nameof(transaction.DealDetails.DealDescription), transaction.DealDetails?.DealDescription ?? string.Empty),

                    new TextSubstitution(nameof(transaction.SpecialTransactionType), SharedIntegration.Resources.SpecialTransactionTypeResource.ResourceManager.GetString(transaction.SpecialTransactionType.ToString())),
                    new TextSubstitution(nameof(transaction.DocumentOrigin), transaction.DocumentOrigin.ToString()),
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

                if (transaction.DealDetails?.ConsumerEmail != null && terminal.Settings.SendTransactionSlipEmailToConsumer == true)
                {
                    var email = new Email
                    {
                        EmailTo = transaction.DealDetails.ConsumerEmail,
                        Subject = emailSubject,
                        TemplateCode = emailTemplateCode,
                        Substitutions = substitutions.ToArray()
                    };

                    await emailSender.SendEmail(email);
                }

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
            catch (Exception e)
            {
                logger.LogError(e, $"{nameof(ProcessTransaction)}: SendTransactionSuccessEmails failed");
            }
        }

        // TODOL this is not is use
        private async Task SendBillingInvoiceSuccessEmails(BillingDeal billingDeal, Invoice invoice, Terminal terminal, string emailTo = null)
        {
            if (terminal.Settings.SendTransactionSlipEmailToConsumer != true && terminal.Settings.SendTransactionSlipEmailToMerchant != true)
            {
                return;
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

            if (billingDeal.DealDetails?.ConsumerEmail != null && terminal.Settings.SendTransactionSlipEmailToConsumer == true)
            {
                var email = new Email
                {
                    EmailTo = billingDeal.DealDetails.ConsumerEmail,
                    Subject = emailSubject,
                    TemplateCode = emailTemplateCode,
                    Substitutions = substitutions.ToArray()
                };

                await emailSender.SendEmail(email);
            }

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

        private async Task<OperationResponse> NextBillingDeal(Terminal terminal, BillingDeal billingDeal, CreditCardTokenKeyVault token)
        {
            var transaction = mapper.Map<CreateTransactionRequest>(billingDeal);

            transaction.CardPresence = CardPresenceEnum.CardNotPresent;
            transaction.TransactionType = TransactionTypeEnum.RegularDeal;

            try
            {
                ActionResult<OperationResponse> actionResult = null;

                if (billingDeal.InvoiceOnly)
                {
                    actionResult = await invoicingController.ProcessBillingInvoice(billingDeal);
                }
                else if (billingDeal.BankDetails != null)
                {
                    actionResult = await ProcessBankTransaction(transaction, specialTransactionType: SpecialTransactionTypeEnum.RegularDeal, billingDeal: billingDeal);
                }
                else
                {
                    actionResult = await ProcessTransaction(terminal, transaction, token, specialTransactionType: SpecialTransactionTypeEnum.RegularDeal, initialTransactionID: billingDeal.InitialTransactionID, billingDeal: billingDeal);
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

        private CreateTransactionRequest ConvertTransactionRequestForRefund(PaymentTransaction transaction, decimal refundAmount)
        {
            var res = mapper.Map<CreateTransactionRequest>(transaction);

            res.TransactionAmount = refundAmount;
            res.NetTotal = null;
            res.VATTotal = null;
            res.Calculate();

            return res;
        }

        // TODO: move to Signal-R service or event handler

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

        // TODO: move to consumers's service call
        private async Task<Guid?> CreateConsumer(CreateTransactionRequest transaction, Guid merchantID)
        {
            var consumer = new Merchants.Business.Entities.Billing.Consumer();

            mapper.Map(transaction.DealDetails, consumer);
            consumer.ConsumerName = (transaction.CardOwnerName ?? transaction.CreditCardSecureDetails?.CardOwnerName) ?? transaction.DealDetails?.ConsumerName;
            consumer.ConsumerEmail = transaction.DealDetails?.ConsumerEmail;
            consumer.ConsumerNationalID = transaction.CardOwnerNationalID ?? transaction.CreditCardSecureDetails?.CardOwnerNationalID;
            consumer.MerchantID = merchantID;
            consumer.Active = true;
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
