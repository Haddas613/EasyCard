﻿using AutoMapper;
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
using SharedHelpers = Shared.Helpers;

namespace Transactions.Api.Controllers
{
    [Route("api/masav")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.TerminalOrMerchantFrontendOrAdmin)]
    [ApiController]
    public class MasavFileController : ApiControllerBase
    {
        private readonly ILogger logger;
        private readonly IMasavFileService masavFileService;
        private readonly IMapper mapper;
        private readonly ITerminalsService terminalsService;
        private readonly IBlobStorageService masavFileSorageService;
        private readonly Shared.ApplicationSettings appSettings;

        public MasavFileController(
            ILogger<MasavFileController> logger,
            IMasavFileService masavFileService,
            IMapper mapper,
            ITerminalsService terminalsService,
            IOptions<Shared.ApplicationSettings> appSettings)
        {
            this.logger = logger;
            this.masavFileService = masavFileService;
            this.mapper = mapper;
            this.terminalsService = terminalsService;

            this.appSettings = appSettings.Value;

            // TODO: remove, make singleton
            this.masavFileSorageService = new BlobStorageService(this.appSettings.PublicStorageConnectionString, this.appSettings.MasavFilesStorageTable, this.logger);
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

        [HttpGet]
        public async Task<ActionResult<SummariesResponse<MasavFileSummary>>> GetMasavFiles([FromQuery] MasavFileFilter filter)
        {
            var query = masavFileService.GetMasavFiles().OrderByDescending(f => f.MasavFileID).Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            var response = new SummariesResponse<MasavFileSummary>();

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

            return Ok(response);
        }

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

        [HttpGet("rows")]
        public async Task<ActionResult<SummariesResponse<MasavFileRowSummary>>> GetMasavFileRows([FromQuery] MasavFileRowFilter filter)
        {
            var query = masavFileService.GetMasavFileRows().Filter(filter);
            var numberOfRecordsFuture = query.DeferredCount().FutureValue();

            var masavFile = EnsureExists(await masavFileService.GetMasavFiles().FirstOrDefaultAsync(f => f.MasavFileID == filter.MasavFileID));

            var response = new SummariesResponse<MasavFileRowSummary>();

            response.Data = await mapper.ProjectTo<MasavFileRowSummary>(query.ApplyPagination(filter)).Future().ToListAsync();

            //TODO: add to entity?
            foreach (var row in response.Data)
            {
                row.Currency = masavFile.Currency;
            }

            response.NumberOfRecords = numberOfRecordsFuture.Value;

            return Ok(response);
        }

        [HttpPost("generate/{terminalID:guid}")]
        public async Task<ActionResult<OperationResponse>> PrepareMasavFile(Guid terminalID)
        {
            var terminal = EnsureExists(await terminalsService.GetTerminal(terminalID));

            var bankDetails = terminal.BankDetails;

            var fileDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, UserCultureInfo.TimeZone).Date;

            long? masavFileID = await masavFileService.GenerateMasavFile(terminal.MerchantID, terminalID, bankDetails.Bank, bankDetails.BankBranch, bankDetails.BankAccount, fileDate);

            if (masavFileID.HasValue)
            {
                var response = new OperationResponse() { EntityID = masavFileID.Value, Message = "Masav file generated" };

                return Ok(response);
            }
            else
            {
                var response = new OperationResponse() { Message = "Msav file not geenrated" };

                return Ok(response);
            }
        }

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

            return Ok(res);
        }

        [HttpPost("setPayed/{masavFileID}")]
        public async Task<ActionResult<OperationResponse>> SetPayed(long masavFileID, DateTime? payedDate)
        {
            await masavFileService.SetMasavFilePayed(masavFileID, payedDate ?? DateTime.UtcNow);

            var response = new OperationResponse() { Message = "Msav file payed" };

            return Ok(response);
        }
    }
}
