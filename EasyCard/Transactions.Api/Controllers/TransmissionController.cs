﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Security.KeyVault.Secrets;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Api.Validation;
using Shared.Business.Exceptions;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Security;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Swashbuckle.AspNetCore.Filters;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Models.Transactions;
using Transactions.Api.Services;
using Transactions.Api.Swagger;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Transactions.Shared;
using Transactions.Shared.Enums;

namespace Transactions.Api.Controllers
{
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("api/transmission")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontend)]
    [ApiController]
    public class TransmissionController : ControllerBase
    {
        private readonly ITransactionsService transactionsService;
        private readonly IMapper mapper;
        private readonly IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage;
        private readonly IAggregatorResolver aggregatorResolver;
        private readonly IProcessorResolver processorResolver;
        private readonly ILogger logger;

        // TODO: service client
        private readonly ITerminalsService terminalsService;

        public TransmissionController(ITransactionsService transactionsService, IKeyValueStorage<CreditCardTokenKeyVault> keyValueStorage, IMapper mapper,
            IAggregatorResolver aggregatorResolver, IProcessorResolver processorResolver, ITerminalsService terminalsService, ILogger<TransactionsApiController> logger)
        {
            this.transactionsService = transactionsService;
            this.keyValueStorage = keyValueStorage;
            this.mapper = mapper;

            this.aggregatorResolver = aggregatorResolver;
            this.processorResolver = processorResolver;
            this.terminalsService = terminalsService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<TransactionSummary>>> GetNotSyncedTransactions([FromQuery] TransactionsFilter filter)
        {
            var query = transactionsService.GetTransactions().Filter(filter);

            var response = new SummariesResponse<TransactionSummary> { NumberOfRecords = await query.CountAsync() };

            response.Data = await mapper.ProjectTo<TransactionSummary>(query.ApplyPagination(filter)).ToListAsync();

            return Ok(response);
        }
    }
}