﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Entities.Terminal;
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
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Billing;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Api.Validation;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared;
using Transactions.Shared.Enums;
using Z.EntityFramework.Plus;

namespace Transactions.Api.Controllers
{
    [Route("api/billing")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrManagerOrAdmin)]
    [ApiController]
    public class BillingController : ApiControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly ICreditCardTokenService creditCardTokenService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage;
        private readonly ApplicationSettings appSettings;
        private readonly ILogger logger;
        private readonly IProcessorResolver processorResolver;
        private readonly IBillingDealService billingDealService;
        private readonly IConsumersService consumersService;
        private readonly ITerminalsService terminalsService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly IQueue billingDealsQueue;
        private readonly ApiSettings apiSettings;
        private readonly IEmailSender emailSender;

        public BillingController(
            ITransactionsService transactionsService,
            ICreditCardTokenService creditCardTokenService,
            IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage,
            IMapper mapper,
            ITerminalsService terminalsService,
            IOptions<ApplicationSettings> appSettings,
            ILogger<CardTokenController> logger,
            IProcessorResolver processorResolver,
            IBillingDealService billingDealService,
            IHttpContextAccessorWrapper httpContextAccessor,
            IConsumersService consumersService,
            IQueueResolver queueResolver,
            IOptions<ApiSettings> apiSettings,
            IEmailSender emailSender)
        {
            this.transactionsService = transactionsService;
            this.creditCardTokenService = creditCardTokenService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;

            this.terminalsService = terminalsService;
            this.appSettings = appSettings.Value;
            this.logger = logger;
            this.processorResolver = processorResolver;
            this.billingDealService = billingDealService;
            this.httpContextAccessor = httpContextAccessor;
            this.consumersService = consumersService;
            this.billingDealsQueue = queueResolver.GetQueue(QueueResolver.BillingDealsQueue);
            this.apiSettings = apiSettings.Value;
            this.emailSender = emailSender;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = (httpContextAccessor.GetUser().IsAdmin() ? typeof(BillingDealSummaryAdmin) : typeof(BillingDealSummary))
                    .GetObjectMeta(BillingDealSummaryResource.ResourceManager, System.Globalization.CultureInfo.InvariantCulture)
            };
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<BillingDealSummary>>> GetBillingDeals([FromQuery] BillingDealsFilter filter)
        {
            var query = billingDealService.GetBillingDeals().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            using (var dbTransaction = billingDealService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                if (httpContextAccessor.GetUser().IsAdmin())
                {
                    var response = new SummariesResponse<BillingDealSummaryAdmin>();

                    var summary = await mapper.ProjectTo<BillingDealSummaryAdmin>(query.OrderByDynamic(filter.SortBy ?? nameof(PaymentRequest.PaymentRequestTimestamp), filter.SortDesc)
                        .ApplyPagination(filter)).ToListAsync();

                    var terminalsId = summary.Select(t => t.TerminalID).Distinct();

                    var terminals = await terminalsService.GetTerminals()
                        .Include(t => t.Merchant)
                        .Where(t => terminalsId.Contains(t.TerminalID))
                        .Select(t => new { t.TerminalID, t.Label, t.Merchant.BusinessName })
                        .ToDictionaryAsync(k => k.TerminalID, v => new { v.Label, v.BusinessName });

                    //TODO: Merchant name instead of BusinessName
                    summary.ForEach(s =>
                    {
                        if (terminals.ContainsKey(s.TerminalID))
                        {
                            s.TerminalName = terminals[s.TerminalID].Label;
                            s.MerchantName = terminals[s.TerminalID].BusinessName;
                        }
                    });

                    response.Data = summary;
                    response.NumberOfRecords = numberOfRecordsFuture.Value;
                    return Ok(response);
                }
                else
                {
                    var response = new SummariesResponse<BillingDealSummary>();

                    //TODO: ordering
                    response.Data = await mapper.ProjectTo<BillingDealSummary>(query.OrderByDynamic(filter.SortBy ?? nameof(BillingDeal.BillingDealTimestamp), filter.SortDesc).ApplyPagination(filter)).Future().ToListAsync();
                    response.NumberOfRecords = numberOfRecordsFuture.Value;

                    return Ok(response);
                }
            }
        }

        [HttpGet]
        [Route("{BillingDealID}")]
        public async Task<ActionResult<BillingDealResponse>> GetBillingDeal([FromRoute] Guid billingDealID)
        {
            using (var dbTransaction = billingDealService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var dbBillingDeal = EnsureExists(await billingDealService.GetBillingDeals().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

                var billingDeal = mapper.Map<BillingDealResponse>(dbBillingDeal);

                if (billingDeal.TerminalID.HasValue)
                {
                    billingDeal.TerminalName = await terminalsService.GetTerminals().Where(t => t.TerminalID == billingDeal.TerminalID.Value).Select(t => t.Label).FirstOrDefaultAsync();
                }

                return Ok(billingDeal);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<OperationResponse>> CreateBillingDeal([FromBody] BillingDealRequest model)
        {
            // TODO: caching
            var terminal = EnsureExists(await terminalsService.GetTerminal(model.TerminalID));
            var consumer = EnsureExists(await consumersService.GetConsumers().FirstOrDefaultAsync(d => d.TerminalID == terminal.TerminalID && d.ConsumerID == model.DealDetails.ConsumerID), "Consumer");
            var token = EnsureExists(await creditCardTokenService.GetTokens().FirstOrDefaultAsync(d => d.TerminalID == terminal.TerminalID && d.CreditCardTokenID == model.CreditCardToken && d.ConsumerID == consumer.ConsumerID), "CreditCardToken");

            var newBillingDeal = mapper.Map<BillingDeal>(model);
            newBillingDeal.Active = true;

            newBillingDeal.InitialTransactionID = token.InitialTransactionID;

            mapper.Map(token, newBillingDeal.CreditCardDetails);

            newBillingDeal.MerchantID = terminal.MerchantID;

            // TODO: calculation

            newBillingDeal.ApplyAuditInfo(httpContextAccessor);

            newBillingDeal.NextScheduledTransaction = newBillingDeal.BillingSchedule.GetInitialScheduleDate();

            await billingDealService.CreateEntity(newBillingDeal);

            return CreatedAtAction(nameof(GetBillingDeal), new { BillingDealID = newBillingDeal.BillingDealID }, new OperationResponse(Messages.BillingDealCreated, StatusEnum.Success, newBillingDeal.BillingDealID));
        }

        [HttpPut]
        [Route("{BillingDealID}")]
        public async Task<ActionResult<OperationResponse>> UpdateBillingDeal([FromRoute] Guid billingDealID, [FromBody] BillingDealUpdateRequest model)
        {
            var billingDeal = EnsureExists(await billingDealService.GetBillingDeals().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

            if (model.IssueInvoice != true)
            {
                model.InvoiceDetails = null;
            }

            mapper.Map(model, billingDeal);

            billingDeal.ApplyAuditInfo(httpContextAccessor);

            //TODO: reschedule only on demand ?
            //billingDeal.NextScheduledTransaction = billingDeal.BillingSchedule.GetInitialScheduleDate();

            await billingDealService.UpdateEntity(billingDeal);

            return Ok(new OperationResponse(Messages.BillingDealUpdated, StatusEnum.Success, billingDealID));
        }

        [HttpDelete]
        [Route("{BillingDealID}")]
        public async Task<ActionResult<OperationResponse>> DeleteBillingDeal([FromRoute] Guid billingDealID)
        {
            var billingDeal = EnsureExists(await billingDealService.GetBillingDeals().FirstOrDefaultAsync(m => m.BillingDealID == billingDealID));

            billingDeal.Active = false;

            billingDeal.ApplyAuditInfo(httpContextAccessor);

            await billingDealService.UpdateEntity(billingDeal);

            return Ok(new OperationResponse(Messages.BillingDealDeleted, StatusEnum.Success, billingDealID));
        }

        [HttpPost]
        [Route("due-billings")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<SendBillingDealsToQueueResponse>> SendDueBillingDeals()
        {
            var filter = new BillingDealsFilter
            {
                OnlyActual = true
            };

            var query = billingDealService.GetBillingDeals().Filter(filter);
            var numberOfRecords = 0;

            using (var dbTransaction = billingDealService.BeginDbTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var response = new SummariesResponse<Guid>();

                var billings = await GetFilteredQueueBillingDeals();

                //var billings = await query.OrderBy(b => b.NextScheduledTransaction)
                //    .Filter(filter)
                //    .Join(creditCardTokenService.GetTokens(), o => o.CreditCardToken, i => i.CreditCardTokenID, (l, r) => new { l.BillingDealID, l.TerminalID, r.CardExpiration })
                //    .ToListAsync();

                var allTerminalsID = billings.Select(b => b.TerminalID).Distinct();

                var terminals = (await terminalsService.GetTerminals().Where(t => allTerminalsID.Contains(t.TerminalID)).ToListAsync())
                    .Where(t => t.EnabledFeatures?.Contains(Merchants.Shared.Enums.FeatureEnum.Billing) == true)
                    .Where(t => t.BillingSettings.CreateRecurrentPaymentsAutomatically == true)
                    .Select(t => t.TerminalID)
                    .ToHashSet();

                var processableBillings = billings.Where(b => terminals.Contains(b.TerminalID)).Select(b => b.BillingDealID);

                numberOfRecords = processableBillings.Count();
                var batch = new List<Guid>(appSettings.BillingDealsMaxBatchSize);
                foreach (var id in processableBillings)
                {
                    if (batch.Count == appSettings.BillingDealsMaxBatchSize)
                    {
                        await billingDealsQueue.PushToQueue<IEnumerable<Guid>>(batch);
                        batch.Clear();
                    }

                    batch.Add(id);
                }

                if (batch.Count > 0)
                {
                    await billingDealsQueue.PushToQueue<IEnumerable<Guid>>(batch);
                }
            }

            return new SendBillingDealsToQueueResponse { Status = StatusEnum.Success, Message = Messages.TransactionsQueued, Count = numberOfRecords };
        }

        /// <summary>
        /// Retrieves billing deals for queue and sends credit card expired emails to customers if required.
        /// </summary>
        /// <returns></returns>
        private async Task<IEnumerable<BillingDealQueueEntry>> GetFilteredQueueBillingDeals()
        {
            var filter = new BillingDealsFilter
            {
                OnlyActual = true
            };

            var query = billingDealService.GetBillingDeals().Filter(filter);

            var allBillings = await query.OrderBy(b => b.NextScheduledTransaction)
                    .Filter(filter)
                    .Join(creditCardTokenService.GetTokens(), o => o.CreditCardToken, i => i.CreditCardTokenID, (l, r) => new { l.BillingDealID, l.TerminalID, r.CardExpiration })
                    .ToListAsync();

            var response = new List<BillingDealQueueEntry>();

            if (allBillings.Count == 0)
            {
                return response;
            }

            var allTerminalsID = allBillings.Select(t => t.TerminalID).Distinct();

            //TODO: optimize
            var terminals = await terminalsService.GetTerminals().Include(t => t.Merchant).Where(t => allTerminalsID.Contains(t.TerminalID)).ToDictionaryAsync(k => k.TerminalID, v => v);

            foreach (var billing in allBillings)
            {
                if (terminals.ContainsKey(billing.TerminalID))
                {
                    var terminal = terminals[billing.TerminalID];

                    if (terminal.BillingSettings.CreateRecurrentPaymentsAutomatically.GetValueOrDefault(false) == false)
                    {
                        continue;
                    }
                }

                if (billing.CardExpiration?.Expired == true)
                {
                    var dealEntity = await billingDealService.GetBillingDealsForUpdate().FirstOrDefaultAsync(b => b.BillingDealID == billing.BillingDealID);

                    logger.LogInformation($"Billing Deal {dealEntity?.BillingDealID} credit card {CreditCardHelpers.GetCardBin(dealEntity?.CreditCardDetails.CardNumber)} has expired ({dealEntity?.CreditCardDetails.CardExpiration}). Setting it as inactive.");

                    if (dealEntity != null)
                    {
                        dealEntity.Active = false;
                        await billingDealService.UpdateEntity(dealEntity);

                        if (terminals.ContainsKey(billing.TerminalID))
                        {
                            await SendBillingDealCreditCardTokenExpiredEmail(dealEntity, terminals[billing.TerminalID]);
                        }
                        else
                        {
                            logger.LogError($"Could not send {nameof(SendBillingDealCreditCardTokenExpiredEmail)}. Terminal {billing.TerminalID} was not present in dictionary.");
                        }
                    }
                }
                else
                {
                    response.Add(new BillingDealQueueEntry { BillingDealID = billing.BillingDealID, TerminalID = billing.TerminalID });
                }
            }

            return response;
        }

        private async Task SendBillingDealCreditCardTokenExpiredEmail(BillingDeal billingDeal, Terminal terminal)
        {
            var settings = terminal.PaymentRequestSettings;

            //TODO: resources?
            var emailSubject = $"{terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName}: - פג תוקף כרטיס אשראי המשמש לעסקה מחזורית";
            var emailTemplateCode = "BillingDealCardExpired";
            var substitutions = new List<TextSubstitution>
            {
                new TextSubstitution(nameof(settings.MerchantLogo), string.IsNullOrWhiteSpace(settings.MerchantLogo) ? $"{apiSettings.CheckoutPortalUrl}/img/merchant-logo.png" : settings.MerchantLogo),
                new TextSubstitution(nameof(terminal.Merchant.MarketingName), terminal.Merchant.MarketingName ?? terminal.Merchant.BusinessName),

                new TextSubstitution(nameof(billingDeal.DealDetails.DealDescription), billingDeal.DealDetails?.DealDescription ?? string.Empty),

                new TextSubstitution(nameof(billingDeal.CreditCardDetails.CardNumber), billingDeal.CreditCardDetails?.CardNumber ?? string.Empty),
                new TextSubstitution(nameof(billingDeal.CreditCardDetails.CardOwnerName), billingDeal.CreditCardDetails?.CardOwnerName ?? string.Empty),
                new TextSubstitution(nameof(billingDeal.DealDetails.ConsumerID), billingDeal.DealDetails.ConsumerID?.ToString() ?? string.Empty),
                new TextSubstitution(nameof(billingDeal.BillingDealID), billingDeal.BillingDealID.ToString()),
            };

            if (!string.IsNullOrWhiteSpace(billingDeal.DealDetails?.ConsumerEmail))
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

            if (terminal.BillingSettings.BillingNotificationsEmails?.Count() > 0)
            {
                var merchantEmailTemplateCode = "BillingDealCardExpiredMerchant";

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
    }
}
