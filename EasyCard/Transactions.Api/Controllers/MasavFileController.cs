using AutoMapper;
using BasicServices.BlobStorage;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PoalimOnlineBusiness;
using Shared.Api;
using Shared.Api.Extensions;
using Shared.Api.Models;
using Shared.Api.Models.Metadata;
using Shared.Api.UI;
using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Extensions.Filtering;
using Transactions.Api.Models.Masav;
using Transactions.Business.Entities;
using Transactions.Business.Services;
using Z.EntityFramework.Plus;
using Transactions.Shared;
using SharedApi = Shared.Api;
using SharedHelpers = Shared.Helpers;
using Microsoft.Extensions.Azure;

namespace Transactions.Api.Controllers
{
    [Route("api/masav")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MasavFileController : ApiControllerBase
    {
        private readonly ILogger logger;
        private readonly IMasavFileService masavFileService;
        private readonly IMapper mapper;
        private readonly ITerminalsService terminalsService;
        private readonly ITransactionsService transactionsService;
        private readonly IBlobStorageService masavFileSorageService;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;
        private readonly Shared.ApplicationSettings appSettings;
        private readonly IBillingDealService billingDealService;

        public MasavFileController(
            ITransactionsService transactionsService,
            ILogger<MasavFileController> logger,
            IMasavFileService masavFileService,
            IMapper mapper,
            ITerminalsService terminalsService,
            IOptions<Shared.ApplicationSettings> appSettings,
            IHttpContextAccessorWrapper httpContextAccessor,
            IBillingDealService billingDealService)
        {
            this.transactionsService = transactionsService;
            this.logger = logger;
            this.masavFileService = masavFileService;
            this.mapper = mapper;
            this.terminalsService = terminalsService;

            this.appSettings = appSettings.Value;
            this.httpContextAccessor = httpContextAccessor;

            // TODO: remove, make singleton
            this.masavFileSorageService = new BlobStorageService(this.appSettings.PublicStorageConnectionString, this.appSettings.MasavFilesStorageTable, this.logger);

            this.billingDealService = billingDealService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta")]
        public TableMeta GetMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(MasavFileSummary)
                    .GetObjectMeta(MasavFileResource.ResourceManager, CurrentCulture)
            };
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("$meta-row")]
        public TableMeta GetRowMetadata()
        {
            return new TableMeta
            {
                Columns = typeof(MasavFileRowSummary)
                    .GetObjectMeta(MasavFileRowResource.ResourceManager, CurrentCulture)
            };
        }

        /// <summary>
        /// Get masav files details
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<SummariesAmountResponse<MasavFileSummary>>> GetMasavFiles([FromQuery] MasavFileFilter filter)
        {
            var query = masavFileService.GetMasavFiles().OrderByDescending(f => f.MasavFileID).Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();
            var totalAmount = new
            {
                ILS = query.Where(e => e.Currency == CurrencyEnum.ILS).DeferredSum(e => e.TotalAmount).FutureValue(),
                USD = query.Where(e => e.Currency == CurrencyEnum.USD).DeferredSum(e => e.TotalAmount).FutureValue(),
                EUR = query.Where(e => e.Currency == CurrencyEnum.EUR).DeferredSum(e => e.TotalAmount).FutureValue(),
            };

            var response = new SummariesAmountResponse<MasavFileSummary>();

            response.Data = await mapper.ProjectTo<MasavFileSummary>(query.ApplyPagination(filter)).Future().ToListAsync();

            var terminalsId = response.Data.Select(t => t.TerminalID).Distinct();

            var terminals = await terminalsService.GetTerminals()
                .Where(t => terminalsId.Contains(t.TerminalID))
                .Select(t => new { t.TerminalID, t.Label })
                .ToDictionaryAsync(k => k.TerminalID, v => new { v.Label });

            foreach (var data in response.Data)
            {
                if (data.TerminalID.HasValue && terminals.ContainsKey(data.TerminalID.Value))
                {
                    data.TerminalName = terminals[data.TerminalID.Value].Label;
                }
            }

            response.NumberOfRecords = numberOfRecordsFuture.Value;
            response.TotalAmountILS = totalAmount.ILS.Value;
            response.TotalAmountUSD = totalAmount.USD.Value;
            response.TotalAmountEUR = totalAmount.EUR.Value;

            return Ok(response);
        }

        /// <summary>
        /// Get masav file per id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:long}")]
        public async Task<ActionResult<MasavFileSummary>> GetMasavFile([FromRoute] long id)
        {
            var dbEntity = EnsureExists(await masavFileService.GetMasavFiles().FirstOrDefaultAsync(m => m.MasavFileID == id));
            var response = mapper.Map<MasavFileSummary>(dbEntity);

            if (response.TerminalID.HasValue)
            {
                var terminal = await terminalsService.GetTerminals().Where(t => t.TerminalID == response.TerminalID.Value).Select(s => s.Label).FirstOrDefaultAsync();

                if (terminal != null)
                {
                    response.TerminalName = terminal;
                }
            }

            return Ok(response);
        }

        /// <summary>
        /// Get masav files rows data
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("rows")]
        public async Task<ActionResult<SummariesResponse<MasavFileRowSummary>>> GetMasavFileRows([FromQuery] MasavFileRowFilter filter)
        {
            var masavFile = EnsureExists(await masavFileService.GetMasavFile(filter.MasavFileID));

            var response = new SummariesResponse<MasavFileRowSummary>();

            var query = masavFile.Rows.AsQueryable().Filter(filter);
            var numberOfRecordsFuture = query.Count();

            response.Data = mapper.ProjectTo<MasavFileRowSummary>(query.ApplyPagination(filter)).ToList();

            //TODO: add to entity?
            foreach (var row in response.Data)
            {
                row.Currency = masavFile.Currency;
            }

            response.NumberOfRecords = numberOfRecordsFuture;

            return Ok(response);
        }

        /// <summary>
        /// Generate masav file
        /// </summary>
        /// <param name="terminalID"></param>
        /// <returns></returns>
        [HttpPost("generate/{terminalID:guid}")]
        public async Task<ActionResult<OperationResponse>> PrepareMasavFile(Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            var bankDetails = terminal.BankDetails;

            if (bankDetails == null)
            {
                var response = new OperationResponse() { Status = SharedApi.Models.Enums.StatusEnum.Error, Message = Shared.Messages.TerminalMustHaveBankDetailsSpecified };
                return BadRequest(response);
            }

            var fileDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;

            long? masavFileID = await masavFileService.GenerateMasavFile(terminal.MerchantID, terminalID, bankDetails.InstituteName.ContainsHebrew() ? string.Empty : bankDetails.InstituteName, bankDetails.InstituteServiceNum, bankDetails.InstituteNum, fileDate);

            if (masavFileID.HasValue)
            {
                var response = new OperationResponse() { Status = SharedApi.Models.Enums.StatusEnum.Success, EntityID = masavFileID.Value, Message = Shared.Messages.MasavFileGeneratedSuccessfully };

                return Ok(response);
            }
            else
            {
                var response = new OperationResponse() { Status = SharedApi.Models.Enums.StatusEnum.Error, Message = Shared.Messages.MasavFileWasNotGenerated };

                return BadRequest(response);
            }
        }

        /// <summary>
        /// Download masav file, return url
        /// </summary>
        /// <param name="masavFileID"></param>
        /// <returns></returns>
        [HttpPost("download/{masavFileID}")]
        public async Task<ActionResult<OperationResponse>> GenerateMasavFile(long masavFileID)
        {
            var masavFile = EnsureExists(await masavFileService.GetMasavFile(masavFileID));

            if (string.IsNullOrWhiteSpace(masavFile.StorageReference))
            {
                MasavDataWithdraw masavData = mapper.Map<MasavDataWithdraw>(masavFile);

                using (var file = new MemoryStream())
                {
                    await masavData.ExportWithdrawFile(file);

                    await file.FlushAsync();

                    var length = file.Length;

                    file.Seek(0, SeekOrigin.Begin);

                    var fileReference = await masavFileSorageService.Upload($"{masavFile.TerminalID}/{masavFile.MasavFileDate:yyyy-MM-dd}-{masavFile.MasavFileID}-{Guid.NewGuid().ToString().Substring(0, 6)}.msv", file);

                    masavFile.StorageReference = fileReference;
                    await masavFileService.UpdateMasavFile(masavFile);
                }
            }

            var res = masavFileSorageService.GetDownloadUrl(masavFile.StorageReference);

            return Ok(new OperationResponse { Status = SharedApi.Models.Enums.StatusEnum.Success, EntityReference = res });
        }

        [HttpPost("removeRows")]
        public async Task<ActionResult<OperationResponse>> RemoveMasavFileRows(MasavFileRowsSetRequest request)
        {
            if (request.MasavFileRowIDs == null || request.MasavFileRowIDs.Count() == 0)
            {
                return BadRequest(new OperationResponse(Messages.RowsRequired, null, httpContextAccessor.TraceIdentifier, nameof(request.MasavFileRowIDs), Messages.RowsRequired));
            }

            var masavFile = EnsureExists(await masavFileService.GetMasavFile(request.MasavFileID));

            using (var dbTransaction = transactionsService.BeginDbTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                try
                {
                    foreach (var rowID in request.MasavFileRowIDs)
                    {
                        var existingRow = masavFile.Rows.Where(d => d.MasavFileRowID == rowID).FirstOrDefault();
                        if (existingRow == null)
                        {
                            await dbTransaction.RollbackAsync();
                            return NotFound("Specified row does not exist");
                        }

                        existingRow.IsPayed = false;

                        var transaction = EnsureExists(
                          await transactionsService.GetTransactionsForUpdate().FirstOrDefaultAsync(m => m.PaymentTransactionID == existingRow.PaymentTransactionID));
                        await transactionsService.UpdateEntityWithStatus(transaction, Shared.Enums.TransactionStatusEnum.CancelledByMerchant, dbTransaction: dbTransaction);

                        if (transaction.BillingDealID != null)
                        {
                            var billing = EnsureExists(await billingDealService.GetBillingDeal(transaction.BillingDealID.Value));
                            billing.RevertLastTransactionForBankBillings(Messages.TransactionCanceled);
                            await billingDealService.UpdateEntityWithHistory(billing, Messages.TransactionCanceled, Shared.Enums.BillingDealOperationCodesEnum.TriggerTransactionFailed, dbTransaction);
                        }
                    }

                    masavFile.StorageReference = null;
                    await masavFileService.UpdateMasavFile(masavFile, dbTransaction);

                    await dbTransaction.CommitAsync();
                }
                catch (Exception)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }

            var response = new OperationResponse(Messages.MasavFileProcessedSuccessfully, SharedApi.Models.Enums.StatusEnum.Success);

            return Ok(response);
        }
    }
}
