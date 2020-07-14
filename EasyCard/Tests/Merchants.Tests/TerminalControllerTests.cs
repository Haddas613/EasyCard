using Merchants.Api.Controllers;
using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.Terminal;
using Merchants.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace MerchantsApi.Tests
{
    [Collection("MerchantsCollection")]
    [Order(2)]
    public class TerminalControllerTests
    {
        private MerchantsFixture merchantsFixture;

        public TerminalControllerTests(MerchantsFixture merchantsFixture)
        {
            this.merchantsFixture = merchantsFixture;
        }

        [Fact(DisplayName = "CreateTerminal: Creates when model is correct")]
        [Order(1)]
        public async Task CreateTerminal_CreatesWhenModelIsCorrect()
        {
            var controller = new TerminalsApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper, merchantsFixture.ExternalSystemsService, null); // TODO: fixture
            var merchant = await merchantsFixture.MerchantsService.GetMerchants().FirstAsync();
            var billingEmails = new List<string> { "mail1@mail.com", "mail2@mail.com" };
            var terminalModel = new TerminalRequest
            {
                Label = Guid.NewGuid().ToString(),
                MerchantID = merchant.MerchantID,
                Settings = new TerminalSettings { MaxInstallments = 20, CvvRequired = true },
                BillingSettings = new TerminalBillingSettings { BillingNotificationsEmails = billingEmails }
            };
            var actionResult = await controller.CreateTerminal(terminalModel);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get newly created terminal
            var terminal = await GetTerminal(new Guid(responseData.EntityReference));
            Assert.NotNull(terminal);
            Assert.NotNull(terminal.Settings);
            Assert.NotNull(terminal.BillingSettings);
            Assert.Equal(terminalModel.Label, terminal.Label);
            Assert.Equal(terminalModel.MerchantID, terminal.MerchantID);
            Assert.Equal(20, terminal.Settings.MaxInstallments);
            Assert.True(terminal.Settings.CvvRequired);
            Assert.True(billingEmails.All(m => terminalModel.BillingSettings.BillingNotificationsEmails.Contains(m)));

            //check if merchant history was updated
            var history = (await merchantsFixture.MerchantsService.GetMerchantHistories().ToListAsync()).
                LastOrDefault(h => h.MerchantID == merchant.MerchantID && h.OperationCode == OperationCodesEnum.TerminalCreated);
            Assert.NotNull(history);
            Assert.NotNull(history.OperationDoneBy);
            Assert.NotNull(history.SourceIP);
            Assert.True(history.OperationDoneByID == merchantsFixture.HttpContextAccessorWrapper.UserIdClaim);
        }

        [Fact(DisplayName = "UpdateTerminal: Updates when model is correct")]
        [Order(2)]
        public async Task UpdateTerminal_UpdatesWhenModelIsCorrect()
        {
            var controller = new TerminalsApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper, merchantsFixture.ExternalSystemsService, null); // TODO: fixture
            var newName = Guid.NewGuid().ToString();
            var existingTerminal = await merchantsFixture.TerminalsService.GetTerminals().FirstOrDefaultAsync();
            var terminalModel = new UpdateTerminalRequest
            {
                Label = newName,
                Settings = new TerminalSettings { MaxInstallments = 50, CvvRequired = false },
                BillingSettings = new TerminalBillingSettings { BillingNotificationsEmails = null }
            };
            var actionResult = await controller.UpdateTerminal(existingTerminal.TerminalID, terminalModel);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get updated terminal
            var terminal = await GetTerminal(new Guid(responseData.EntityReference));
            Assert.NotNull(terminal);
            Assert.Equal(terminalModel.Label, terminal.Label);
            Assert.Equal(50, terminal.Settings.MaxInstallments);
            Assert.False(terminal.Settings.CvvRequired);
            Assert.True(terminal.BillingSettings.BillingNotificationsEmails.Count() == 0);

            //check if merchant history was updated
            var history = (await merchantsFixture.MerchantsService.GetMerchantHistories().ToListAsync()).
                LastOrDefault(h => h.MerchantID == existingTerminal.MerchantID && h.OperationCode == OperationCodesEnum.TerminalUpdated);
            Assert.NotNull(history);
            Assert.NotNull(history.OperationDoneBy);
            Assert.NotNull(history.SourceIP);
            Assert.NotNull(history.OperationDescription);
            Assert.True(history.OperationDoneByID == merchantsFixture.HttpContextAccessorWrapper.UserIdClaim);
        }

        [Fact(DisplayName = "GetTerminals: Returns collection of Terminals")]
        [Order(3)]
        public async Task GetTerminals_ReturnsCollectionOfTerminals()
        {
            var controller = new TerminalsApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper, merchantsFixture.ExternalSystemsService, null); // TODO: fixture
            var filter = new TerminalsFilter();
            var actionResult = await controller.GetTerminals(filter);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as SummariesResponse<TerminalSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.True(responseData.NumberOfRecords > 0);
        }

        [Fact(DisplayName = "GetTerminals: Filters collection of Terminals")]
        [Order(4)]
        public async Task GetTerminals_FiltersAndReturnsCollectionOfTerminals()
        {
            var controller = new TerminalsApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper, merchantsFixture.ExternalSystemsService, null); // TODO: fixture
            var filter = new TerminalsFilter { Label = Guid.NewGuid().ToString() }; //assumed unique non-taken name
            var actionResult = await controller.GetTerminals(filter);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as SummariesResponse<TerminalSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.True(responseData.NumberOfRecords == 0);

            var existingTerminal = await merchantsFixture.TerminalsService.GetTerminals().FirstOrDefaultAsync();
            actionResult = await controller.GetTerminals(new TerminalsFilter { Label = existingTerminal.Label });
            response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            responseData = response.Value as SummariesResponse<TerminalSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.True(responseData.NumberOfRecords == 1); //assuming the name is unique
        }

        [Fact(DisplayName = "TerminalExternalSystem: Creates when model is correct")]
        [Order(5)]
        public async Task TerminalExternalSystem_CreatesWhenModelIsCorrect()
        {
            var controller = new TerminalsApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper, merchantsFixture.ExternalSystemsService, null); // TODO: fixture
            var dictionariesController = new DictionariesApiController(merchantsFixture.Mapper, merchantsFixture.ExternalSystemsService);

            var existingTerminal = await merchantsFixture.TerminalsService.GetTerminals().FirstOrDefaultAsync();
            var existingExternalSystem = merchantsFixture.ExternalSystemsService.GetExternalSystems().FirstOrDefault() ?? throw new Exception("No external systems available");

            var terminalExternalSystemRequest = new ExternalSystemRequest
            {
                ExternalSystemID = existingExternalSystem.ExternalSystemID,
                ExternalProcessorReference = "test",
                Settings = JObject.FromObject(new { SomeSetting = "123" })
            };
            var actionResult = await controller.SaveTerminalExternalSystem(existingTerminal.TerminalID, terminalExternalSystemRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get updated terminal
            var externalSystemResponse = (await GetTerminal(new Guid(responseData.EntityReference))).Integrations
                .FirstOrDefault(e => e.ExternalSystemID == existingExternalSystem.ExternalSystemID) ?? throw new Exception("No external system in GetTerminal response");

            Assert.Equal(terminalExternalSystemRequest.ExternalProcessorReference, externalSystemResponse.ExternalProcessorReference);
            Assert.Equal("123", externalSystemResponse.Settings["SomeSetting"].Value<string>());
        }

        [Fact(DisplayName = "TerminalExternalSystem: Updates when model is correct")]
        [Order(6)]
        public async Task TerminalExternalSystem_UpdatesWhenModelIsCorrect()
        {
            var controller = new TerminalsApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper, merchantsFixture.ExternalSystemsService, null); // TODO: fixture
            var dictionariesController = new DictionariesApiController(merchantsFixture.Mapper, merchantsFixture.ExternalSystemsService);

            var existingTerminal = await merchantsFixture.TerminalsService.GetTerminals().FirstOrDefaultAsync();
            var existingTerminalExternalSystem = merchantsFixture.TerminalsService.GetTerminalExternalSystems()
                .FirstOrDefault(e => e.TerminalID == existingTerminal.TerminalID) ?? throw new Exception("No terminal external systems available");

            var terminalExternalSystemRequest = new ExternalSystemRequest
            {
                ExternalSystemID = existingTerminalExternalSystem.ExternalSystemID,
                ExternalProcessorReference = "new reference",
                Settings = JObject.FromObject(new { SomeSetting = "456", SomeNewSetting = "Test" })
            };
            var actionResult = await controller.SaveTerminalExternalSystem(existingTerminal.TerminalID, terminalExternalSystemRequest);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(200, response.StatusCode);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get updated terminal
            var externalSystemResponse = (await GetTerminal(new Guid(responseData.EntityReference))).Integrations
                .FirstOrDefault(e => e.ExternalSystemID == existingTerminalExternalSystem.ExternalSystemID) ?? throw new Exception("No external system in GetTerminal response");

            Assert.Equal(terminalExternalSystemRequest.ExternalProcessorReference, externalSystemResponse.ExternalProcessorReference);
            Assert.Equal("456", externalSystemResponse.Settings["SomeSetting"].Value<string>());
            Assert.Equal("Test", externalSystemResponse.Settings["SomeNewSetting"].Value<string>());
        }

        private async Task<TerminalResponse> GetTerminal(Guid terminalID)
        {
            var controller = new TerminalsApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper, merchantsFixture.ExternalSystemsService, null); // TODO: fixture

            var actionResult = await controller.GetTerminal(terminalID);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as TerminalResponse;

            return responseData;
        }
    }
}
