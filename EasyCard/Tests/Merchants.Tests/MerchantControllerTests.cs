using Merchants.Api.Controllers;
using Merchants.Api.Models.Audit;
using Merchants.Api.Models.Merchant;
using Merchants.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
using Shared.Business.Audit;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace MerchantsApi.Tests
{
    [Collection("MerchantsCollection")]
    [Order(1)]
    public class MerchantControllerTests
    {
        private MerchantsFixture merchantsFixture;

        public MerchantControllerTests(MerchantsFixture merchantsFixture)
        {
            this.merchantsFixture = merchantsFixture;
        }

        [Fact(DisplayName = "CreateMerchant: Creates when model is correct")]
        [Order(1)]
        public async Task CreateMerchant_CreatesWhenModelIsCorrect()
        {
            var controller = GetMerchantApiController();
            var merchantModel = new MerchantRequest { BusinessName = Guid.NewGuid().ToString() };
            var actionResult = await controller.CreateMerchant(merchantModel);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get newly created merchant
            var merchant = await GetMerchant(new Guid(responseData.EntityReference));
            Assert.NotNull(merchant);
            Assert.Equal(merchantModel.BusinessName, merchant.BusinessName);

            //check if merchant history was created
            var history = (await merchantsFixture.MerchantsService.GetMerchantHistories().ToListAsync()).
               LastOrDefault(h => h.MerchantID == merchant.MerchantID && h.OperationCode == OperationCodesEnum.MerchantCreated);
            Assert.NotNull(history);
            Assert.NotNull(history.OperationDoneBy);
            Assert.NotNull(history.SourceIP);
            Assert.True(history.OperationDoneByID == merchantsFixture.HttpContextAccessorWrapper.UserIdClaim);
        }

        [Fact(DisplayName = "UpdateMerchant: Updates when model is correct")]
        [Order(2)]
        public async Task UpdateMerchant_UpdatesWhenModelIsCorrect()
        {
            var controller = GetMerchantApiController();
            var newName = Guid.NewGuid().ToString();
            var existingMerchant = await merchantsFixture.MerchantsService.GetMerchants().FirstOrDefaultAsync();
            var merchantModel = new UpdateMerchantRequest { BusinessName = newName };
            var actionResult = await controller.UpdateMerchant(existingMerchant.MerchantID, merchantModel);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get updated merchant
            var merchant = await GetMerchant(existingMerchant.MerchantID);
            Assert.NotNull(merchant);
            Assert.Equal(merchantModel.BusinessName, merchant.BusinessName);

            //check if merchant history was updated
            var history = (await merchantsFixture.MerchantsService.GetMerchantHistories().ToListAsync()).
                LastOrDefault(h => h.MerchantID == merchant.MerchantID && h.OperationCode == OperationCodesEnum.MerchantUpdated);
            Assert.NotNull(history);
            Assert.NotNull(history.OperationDoneBy);
            Assert.NotNull(history.SourceIP);
            Assert.NotNull(history.OperationDescription);
            Assert.True(history.OperationDoneByID == merchantsFixture.HttpContextAccessorWrapper.UserIdClaim);
        }

        [Fact(DisplayName = "GetMerchants: Returns collection of merchants")]
        [Order(3)]
        public async Task GetMerchants_ReturnsCollectionOfMerchants()
        {
            var controller = GetMerchantApiController();
            var filter = new MerchantsFilter();
            var actionResult = await controller.GetMerchants(filter);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as SummariesResponse<MerchantSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.True(responseData.NumberOfRecords > 0);
        }

        [Fact(DisplayName = "GetMerchants: Filters collection of merchants")]
        [Order(4)]
        public async Task GetMerchants_FiltersAndReturnsCollectionOfMerchants()
        {
            var controller = GetMerchantApiController();
            var filter = new MerchantsFilter { Search = Guid.NewGuid().ToString() }; //assumed unique non-taken name
            var actionResult = await controller.GetMerchants(filter);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as SummariesResponse<MerchantSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.True(responseData.NumberOfRecords == 0);

            var existingMerchant = await merchantsFixture.MerchantsService.GetMerchants().FirstOrDefaultAsync();
            actionResult = await controller.GetMerchants(new MerchantsFilter { Search = existingMerchant.BusinessName });
            response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            responseData = response.Value as SummariesResponse<MerchantSummary>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.True(responseData.NumberOfRecords == 1); //assuming the name is unique
        }

        [Fact(DisplayName = "Audit: GetMerchantHistory return result")]
        [Order(5)]
        public async Task Audit_GetMerchantHistoryReturnsResult()
        {
            var controller = new AuditApiController(merchantsFixture.Mapper, merchantsFixture.MerchantsService);
            var referenceHistory = merchantsFixture.MerchantsService.GetMerchantHistories().FirstOrDefault(h => h.MerchantID != null)
                ?? throw new Exception("Couldn't get reference merchant id");

            var filter = new AuditFilter { MerchantID = referenceHistory.MerchantID.Value };
            var actionResult = await controller.Get(filter);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as SummariesResponse<AuditEntryResponse>;

            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.True(responseData.NumberOfRecords > 0);
        }

        private async Task<MerchantResponse> GetMerchant(Guid merchantID)
        {
            var controller = GetMerchantApiController();

            var actionResult = await controller.GetMerchant(merchantID);

            var response = actionResult.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
            var responseData = response.Value as MerchantResponse;

            return responseData;
        }

        private MerchantApiController GetMerchantApiController()
        {
            return new MerchantApiController(
                merchantsFixture.MerchantsService,
                merchantsFixture.Mapper,
                merchantsFixture.TerminalsService,
                merchantsFixture.ApplicationSettings,
                merchantsFixture.ImpersonationService,
                null);
        }
    }
}
