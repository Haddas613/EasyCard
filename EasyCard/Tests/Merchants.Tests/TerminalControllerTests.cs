using Merchants.Api.Controllers;
using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.Terminal;
using Merchants.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
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
            var controller = new TerminalApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper);
            var merchant = await merchantsFixture.MerchantsService.GetMerchants().FirstAsync();
            var terminalModel = new TerminalRequest { Label = Guid.NewGuid().ToString(), MerchantID = merchant.MerchantID };
            var actionResult = await controller.CreateTerminal(terminalModel);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get newly created terminal
            var terminal = await GetTerminal(responseData.EntityID.Value);
            Assert.NotNull(terminal);
            Assert.Equal(terminalModel.Label, terminal.Label);
            Assert.Equal(terminalModel.MerchantID, terminal.MerchantID);
        }

        [Fact(DisplayName = "UpdateTerminal: Updates when model is correct")]
        [Order(2)]
        public async Task UpdateTerminal_UpdatesWhenModelIsCorrect()
        {
            var controller = new TerminalApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper);
            var newName = Guid.NewGuid().ToString();
            var existingTerminal = await merchantsFixture.TerminalsService.GetTerminals().FirstOrDefaultAsync();
            var terminalModel = new UpdateTerminalRequest { Label = newName };
            var actionResult = await controller.UpdateTerminal(existingTerminal.TerminalID, terminalModel);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get updated terminal
            var terminal = await GetTerminal(responseData.EntityID.Value);
            Assert.NotNull(terminal);
            Assert.Equal(terminalModel.Label, terminal.Label);
        }

        [Fact(DisplayName = "GetTerminals: Returns collection of Terminals")]
        [Order(3)]
        public async Task GetTerminals_ReturnsCollectionOfTerminals()
        {
            var controller = new TerminalApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper);
            var filter = new TerminalsFilter();
            var actionResult = await controller.GetTerminals(filter);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as SummariesResponse<TerminalSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.True(responseData.NumberOfRecords > 0);
        }

        [Fact(DisplayName = "GetTerminals: Filters collection of Terminals")]
        [Order(4)]
        public async Task GetTerminals_FiltersAndReturnsCollectionOfTerminals()
        {
            var controller = new TerminalApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper);
            var filter = new TerminalsFilter { Label = Guid.NewGuid().ToString() }; //assumed unique non-taken name
            var actionResult = await controller.GetTerminals(filter);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as SummariesResponse<TerminalSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.True(responseData.NumberOfRecords == 0);

            var existingTerminal = await merchantsFixture.TerminalsService.GetTerminals().FirstOrDefaultAsync();
            actionResult = await controller.GetTerminals(new TerminalsFilter { Label = existingTerminal.Label });
            response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            responseData = response.Value as SummariesResponse<TerminalSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.True(responseData.NumberOfRecords == 1); //assuming the name is unique
        }

        private async Task<MerchantResponse> GetMerchant(long merchantID)
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService, merchantsFixture.Mapper);

            var actionResult = await controller.GetMerchant(merchantID);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as MerchantResponse;

            return responseData;
        }

        private async Task<TerminalResponse> GetTerminal(long terminalID)
        {
            var controller = new TerminalApiController(merchantsFixture.MerchantsService, merchantsFixture.TerminalsService, merchantsFixture.Mapper);

            var actionResult = await controller.GetTerminal(terminalID);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as TerminalResponse;

            return responseData;
        }
    }
}
