using MerchantsApi.Controllers;
using MerchantsApi.Models.Merchant;
using MerchantsApi.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace MerchantsApi.Tests
{
    [Collection("MerchantsCollection"), Order(1)]
    public class MerchantControllerTests
    {
        private MerchantsFixture merchantsFixture;

        public MerchantControllerTests(MerchantsFixture merchantsFixture)
        {
            this.merchantsFixture = merchantsFixture;
        }

        [Fact(DisplayName = "GetMerchant: 404 when merchant with given id does not exist"), Order(1)]
        public async Task GetMerchant_Returns404WhenMerchantDoesNotExists()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService);

            var actionResult = await controller.GetMerchant(-1);

            var response = actionResult as Microsoft.AspNetCore.Mvc.NotFoundObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(404, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Error, responseData.Status);
            Assert.NotNull(responseData.Message);
        }

        [Fact(DisplayName = "CreateMerchant: 400 when model is incorrect"), Order(2)]
        public async Task CreateMerchant_Returns400WhenModelIsIncorrect()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService);
            var merchantModel = new MerchantRequest();
            var actionResult = await controller.CreateMerchant(merchantModel);

            var response = actionResult as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(400, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Error, responseData.Status);
            Assert.NotNull(responseData.Message);
        }

        [Fact(DisplayName = "CreateMerchant: Creates when model is correct"), Order(3)]
        public async Task CreateMerchant_CreatesWhenModelIsCorrect()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService);
            var merchantModel = new MerchantRequest { BusinessName = Guid.NewGuid().ToString() };
            var actionResult = await controller.CreateMerchant(merchantModel);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get newly created merchant
            var merchant = await GetMerchant(responseData.EntityID.Value);
            Assert.NotNull(merchant);
            Assert.Equal(merchantModel.BusinessName, merchant.BusinessName);
        }

        [Fact(DisplayName = "UpdateMerchant: 404 when merchant cannot be found"), Order(4)]
        public async Task UpdateMerchant_Returns404WhenMerchantNotFound()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService);
            var merchantModel = new UpdateMerchantRequest();
            var actionResult = await controller.UpdateMerchant(-1, merchantModel);

            var response = actionResult as Microsoft.AspNetCore.Mvc.NotFoundObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(404, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Error, responseData.Status);
            Assert.NotNull(responseData.Message);
        }

        [Fact(DisplayName = "UpdateMerchant: 400 when model is incorrect"), Order(5)]
        public async Task UpdateMerchant_Returns400WhenModelIsIncorrect()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService);
            var merchantModel = new UpdateMerchantRequest();
            var existingMerchant = await merchantsFixture.MerchantsService.GetMerchants().FirstOrDefaultAsync();

            var actionResult = await controller.UpdateMerchant(existingMerchant.MerchantID, merchantModel);

            var response = actionResult as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(400, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Error, responseData.Status);
            Assert.NotNull(responseData.Message);
        }

        [Fact(DisplayName = "UpdateMerchant: Updates when model is correct"), Order(6)]
        public async Task UpdateMerchant_CreatesWhenModelIsCorrect()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService);
            var newName = Guid.NewGuid().ToString();
            var existingMerchant = await merchantsFixture.MerchantsService.GetMerchants().FirstOrDefaultAsync();
            var merchantModel = new UpdateMerchantRequest { BusinessName = newName };
            var actionResult = await controller.UpdateMerchant(existingMerchant.MerchantID, merchantModel);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as OperationResponse;

            Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get updated merchant
            var merchant = await GetMerchant(existingMerchant.MerchantID);
            Assert.NotNull(merchant);
            Assert.Equal(merchantModel.BusinessName, merchant.BusinessName);
        }

        [Fact(DisplayName = "GetMerchants: Returns collection of merchants"), Order(7)]
        public async Task GetMerchants_ReturnsCollectionOfMerchants()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService);
            var filter = new MerchantsFilter();
            var actionResult = await controller.GetMerchants(filter);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as SummariesResponse<MerchantSummary>;

            Assert.Equal(200, response.StatusCode);
            Assert.True(responseData.NumberOfRecords > 0);
        }

        [Fact(DisplayName = "GetMerchants: Filters collection of merchants"), Order(8)]
        public async Task GetMerchants_FiltersAndReturnsCollectionOfMerchants()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService);
            var filter = new MerchantsFilter { BusinessName = Guid.NewGuid().ToString()}; //assumed unique non-taken name
            var actionResult = await controller.GetMerchants(filter);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as SummariesResponse<MerchantSummary>;

            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.True(responseData.NumberOfRecords == 0);

            var existingMerchant = await merchantsFixture.MerchantsService.GetMerchants().FirstOrDefaultAsync();
            actionResult = await controller.GetMerchants(new MerchantsFilter { BusinessName = existingMerchant.BusinessName });
            response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            responseData = response.Value as SummariesResponse<MerchantSummary>;

            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.True(responseData.NumberOfRecords == 1); //assuming the name is unique
        }

        #region NotTests
        private async Task<MerchantResponse> GetMerchant(long merchantID)
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService);

            var actionResult = await controller.GetMerchant(merchantID);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as MerchantResponse;

            return responseData;
        }
        #endregion
    }
}
