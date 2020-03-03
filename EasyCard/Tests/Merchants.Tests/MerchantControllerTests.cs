using Merchants.Api.Controllers;
using Merchants.Api.Models.Merchant;
using Merchants.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shared.Api.Models;
using Shared.Api.Models.Enums;
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
            var controller = new MerchantApiController(merchantsFixture.MerchantsService, merchantsFixture.Mapper);
            var merchantModel = new MerchantRequest { BusinessName = Guid.NewGuid().ToString() };
            var actionResult = await controller.CreateMerchant(merchantModel);

            var responseData = actionResult.Value;

            //Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get newly created merchant
            var merchant = await GetMerchant(responseData.EntityID.Value);
            Assert.NotNull(merchant);
            Assert.Equal(merchantModel.BusinessName, merchant.BusinessName);
        }

        [Fact(DisplayName = "UpdateMerchant: Updates when model is correct")]
        [Order(2)]
        public async Task UpdateMerchant_UpdatesWhenModelIsCorrect()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService, merchantsFixture.Mapper);
            var newName = Guid.NewGuid().ToString();
            var existingMerchant = await merchantsFixture.MerchantsService.GetMerchants().FirstOrDefaultAsync();
            var merchantModel = new UpdateMerchantRequest { BusinessName = newName };
            var actionResult = await controller.UpdateMerchant(existingMerchant.MerchantID, merchantModel);

            var responseData = actionResult.Value;

            //Assert.Equal(201, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.Equal(StatusEnum.Success, responseData.Status);
            Assert.NotNull(responseData.Message);

            //get updated merchant
            var merchant = await GetMerchant(existingMerchant.MerchantID);
            Assert.NotNull(merchant);
            Assert.Equal(merchantModel.BusinessName, merchant.BusinessName);
        }

        [Fact(DisplayName = "GetMerchants: Returns collection of merchants")]
        [Order(3)]
        public async Task GetMerchants_ReturnsCollectionOfMerchants()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService, merchantsFixture.Mapper);
            var filter = new MerchantsFilter();
            var actionResult = await controller.GetMerchants(filter);

            var responseData = actionResult.Value;

            //Assert.Equal(200, response.StatusCode);
            Assert.True(responseData.NumberOfRecords > 0);
        }

        [Fact(DisplayName = "GetMerchants: Filters collection of merchants")]
        [Order(4)]
        public async Task GetMerchants_FiltersAndReturnsCollectionOfMerchants()
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService, merchantsFixture.Mapper);
            var filter = new MerchantsFilter { BusinessName = Guid.NewGuid().ToString() }; //assumed unique non-taken name
            var actionResult = await controller.GetMerchants(filter);

            var responseData = actionResult.Value;

            //Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.True(responseData.NumberOfRecords == 0);

            var existingMerchant = await merchantsFixture.MerchantsService.GetMerchants().FirstOrDefaultAsync();
            actionResult = await controller.GetMerchants(new MerchantsFilter { BusinessName = existingMerchant.BusinessName });

            responseData = actionResult.Value;

            //Assert.Equal(200, response.StatusCode);
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.Data);
            Assert.True(responseData.NumberOfRecords == 1); //assuming the name is unique
        }

        private async Task<MerchantResponse> GetMerchant(long merchantID)
        {
            var controller = new MerchantApiController(merchantsFixture.MerchantsService, merchantsFixture.Mapper);

            var actionResult = await controller.GetMerchant(merchantID);

            var responseData = actionResult.Value;

            return responseData;
        }
    }
}
