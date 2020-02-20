using MerchantsApi.Controllers;
using MerchantsApi.Models.Merchant;
using MerchantsApi.Tests.Fixtures;
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

        [Fact(DisplayName = "GetMerchant Returns 404 merchant with given id does not exist"), Order(1)]
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

        [Fact(DisplayName = "CreateMerchant Returns 201 and creates when model is correct"), Order(2)]
        public async Task CreateMerchant_Returns201AndCreatesWhenModelIsCorrect()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService);
            var merchantModel = new Models.Merchant.MerchantRequest { BusinessName = Guid.NewGuid().ToString() };
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

        private async Task<MerchantResponse> GetMerchant(long merchantID)
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService);

            var actionResult = await controller.GetMerchant(merchantID);

            var response = actionResult as Microsoft.AspNetCore.Mvc.JsonResult;
            var responseData = response.Value as MerchantResponse;

            return responseData;
        }
    }
}
