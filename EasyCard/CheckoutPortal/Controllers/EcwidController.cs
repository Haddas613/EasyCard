using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CheckoutPortal.Models;
using Transactions.Api.Client;
using Shared.Helpers.Security;
using Shared.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Transactions.Api.Models.Checkout;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Shared.Integration.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Shared.Api.Configuration;
using CheckoutPortal.Models.Ecwid;
using Ecwid.Configuration;
using Ecwid;
using Ecwid.Models;
using MerchantProfileApi.Client;
using MerchantProfileApi.Models.Billing;
using CheckoutPortal.Services;
using CheckoutPortal.Helpers;

namespace CheckoutPortal.Controllers
{
    [AllowAnonymous]
    [Route("ecwid")]
    public class EcwidController : Controller
    {
        private readonly ILogger<EcwidController> logger;
        private readonly IApiClientsFactory apiClientsFactory; 
        private readonly ITerminalApiKeyTokenServiceFactory terminalApiKeyTokenServiceFactory;
        private readonly IMapper mapper;
        private readonly ApiSettings apiSettings;
        private readonly EcwidGlobalSettings ecwidSettings;

        private readonly EcwidConvertor ecwidConvertor;

        public EcwidController(
            ILogger<EcwidController> logger,
            IApiClientsFactory apiClientsFactory,
            ITerminalApiKeyTokenServiceFactory terminalApiKeyTokenServiceFactory,
            IMapper mapper,
            IOptions<ApiSettings> apiSettings,
            IOptions<EcwidGlobalSettings> ecwidSettings)
        {
            this.logger = logger;
            this.apiClientsFactory = apiClientsFactory;
            this.mapper = mapper;
            this.apiSettings = apiSettings.Value;
            this.ecwidSettings = ecwidSettings.Value;
            this.terminalApiKeyTokenServiceFactory = terminalApiKeyTokenServiceFactory;

            ecwidConvertor = new EcwidConvertor(this.ecwidSettings);
        }

        [HttpPost]
        public async Task<IActionResult> Index(EcwidRequestPayload request)
        {
            EcwidPayload ecwidPayload = null;

            try
            {
                ecwidPayload = ecwidConvertor.DecryptEcwidPayload(request.Data);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(EcwidController)} index error: Could not decrypt given request: {request.Data}");
                throw new ApplicationException("Invalid request");
            }

            if (ecwidPayload.MerchantAppSettings == null || string.IsNullOrEmpty(ecwidPayload.MerchantAppSettings?.ApiKey))
            {
                logger.LogError($"{nameof(EcwidController)} index error: Could not retrieve merchant settings: {request.Data}");
                throw new ApplicationException("Invalid request. Merchant settings are not present");
            }

            var tokensService = terminalApiKeyTokenServiceFactory.CreateTokenService(ecwidPayload.MerchantAppSettings.ApiKey);
            var merchantMetadataApiClient = apiClientsFactory.GetMerchantMetadataApiClient(tokensService);

            Guid? consumerID = null;

            var customerPayload = ecwidPayload.Cart.Order.GetConsumerRequest();
            var paymentRequestPayload = ecwidPayload.GetCreatePaymentRequest();

            if (customerPayload != null)
            {
                var exisingConsumers = await merchantMetadataApiClient.GetConsumers(new ConsumersFilter
                {
                    NationalID = customerPayload.ConsumerNationalID,
                    Email = customerPayload.ConsumerEmail,
                    Phone = customerPayload.ConsumerPhone,
                    Origin = customerPayload.Origin,
                });

                if (exisingConsumers.NumberOfRecords == 1)
                {
                    consumerID = exisingConsumers.Data.First().ConsumerID;
                }
                else if (exisingConsumers.NumberOfRecords > 1)
                {
                    // TODO: proper handler
                    throw new ApplicationException("There are several consumers with same Consumer code in ECNG");
                }

                if (consumerID == null)
                {
                    var createConsumerResponse = await merchantMetadataApiClient.CreateConsumer(customerPayload);
                    var existingConsumer = await merchantMetadataApiClient.GetConsumer(createConsumerResponse.EntityUID.GetValueOrDefault());
                    consumerID = existingConsumer.ConsumerID;
                    paymentRequestPayload.DealDetails.ConsumerExternalReference = existingConsumer.ExternalReference;
                }
            }

            paymentRequestPayload.DealDetails.ConsumerID = consumerID;

            var transactionsApiClient = apiClientsFactory.GetTransactionsApiClient(tokensService);

            var paymentIntentResponse = await transactionsApiClient.CreatePaymentIntent(paymentRequestPayload);

            if (paymentIntentResponse.Status != Shared.Api.Models.Enums.StatusEnum.Success)
            {
                throw new ApplicationException(paymentIntentResponse.Message);
            }

            var url = paymentIntentResponse.AdditionalData.Value<string>("url");

            return Redirect(url);
        }

        [Route("settings")]
        [HttpGet]
        public async Task<IActionResult> Settings(EcwidSettingsQuery request)
        {
            return View(request);
        }

        //[Route("test")]
        //[HttpGet]
        //public async Task<IActionResult> Test()
        //{
        //    return await Index(new EcwidRequestPayload
        //    {
        //        Data = "nK4Y2o2rT6YIWQYBbgkfeL_AYtQIxDg0-MRzaVQyS6NZ5cKZhxXSzHQEJXv918Y9QaM02jyjgyDFdUfg2B40g7IyyAdbw5TEW_Hkn2h8liO9bHexlaep9O7i8tkZsxYmnFW6abYSVgbl9hYvK7hXn-fsv1QUZh_Y2G2slamYWU4bmV-8wkSywclMvhHMAXeV9JkBpQP_ivevmBPJm2-ejUkdxNtU9L5nePbkKHpsjHlooEci_8ugs5PZqxCMunk_HO1CEyEvgVhzYgpdUoXKDWV6lpYOfZTXiZq2qHnyWJzccMyXz8DPRa7isVJ6dJ0t44BB0mRVn4L0qNZB1nulbVNpuKRG1ByoP4LOy7JAt0Bb_j10GpPpQPI0rIS2BbwU_vcb4T3UjlhGuaUj8sDR1HaWCzmcyX5dRpBonn3PMatHfzozUcQita8NlbXOHVOPqVWoHObhCeFjsSxR8B_z64H7ju9UTR9XqzKyWDF00b-8boe_nQ8rz0JK-EOjPK3WoTY1Z52Ry8VrLsnKJ4xjrPvOhGVv4x8dqLIlpQFL40zsjch-Ma7WRXrvXSVXVDf8Zb5hCsaik5Qd4d7Hyzs6Q38bi8Ei5AjJkmxze7a6UBns43CNZv2TvvLDyBkphSLkMlvvoCOtJBY-HZyNNdoe1hk4URjwSejJH-bvLlMgNTrTV4ykPVAZ547Dyr5l2cnj_bzlx1eeA5LzuN_yXshuxcPjqt0RJ-C25lMIocosqOuqKe5wdsF0pITlY-vZQCMAmvSGSsqxg2m23PMBtZCgUUte5UDC9i56wbcRMCUPw9xKd-h73nlA9EZ1dN0zXK25O9yZ0pKKuNIkK5SyO9bRTh8fiVEgoKIqBsbd2Dd8u5AHKd40WTviGmSI0v04eauevoEbWOhlFg_ntuzD0Ni8uAEC6NpvTuUclyw3OVjNss4iUq_Fg6-KFx-a7rwFF7WKArtMx9WOllT-TkUmErd1BKXp7OqwDxnb6BOgqvEiuWZKi677J2Gw1ICi5qynb5ttAm3f_2ttmiqxKyE2oWPQbg3NtB7eoGVvcxREr880_Jan9qcmrTxAxUPlG-uq7b2KZlRm1gYHLNXATEWlpp_78vRFT-D5iH86gkdKwAs-Nhv0sY_OWsq7ZTkZovtfwvFdDQakxtwMU0wNy6P_ehMCwqV5XmbFqj5yfm4bVIFGEB_o0xwlvobd_167LFNu3FpgapxLkEwapL3BjRLKuDN1oMdpI1droc4e2LmRGLDrezsU09h5CRuTL1lU4gfYZSMPlQezHqftribkyv-Lsj70WKmzNK3yKrKrdb8RzzKO08tXjqZ-A7J62CJeulkslqOneWgjvP0acSzYs0N903LQZMJ_X-mBns66rr4S1SIBwg7sR3tBEyOf4Q2ZnH2ITEz1_VtmXpdbQ_AjniYGJwwTW0NvHLkvD7hqmVcO0l7vyQbvsoDiVblofJJ2C5bhxGWV6atxwk3L2ZTwUy-35auHhoTrfsiLXIqZ-U0lH4O8D-rDEhAVcrMMIjmDC4Q424Swa-T8FWbKDzLA8Zt5dWUQDZIpcGes1Zun3AMNCDKqDqXm59ze9DCd-WZNGSi4MCAhQsLcE6z61gv4ZeMaUKb6SFMLXrCAFIqKULZ0Tq_nSvzDgg_3LR1UPrd2Pvy7N_SjjuiEbTd9DCuumx79FnOJIJUBZkt7vhnvtHB-1IsFvvHDfDBGPlhHHVTivYjEH74LExDXbtJP5epStKPJJ3E0uxgQCP-6Xw4QjcuMMmmz8Zb2hW_3FAJlpxOy-7nc5kLYL-9-60E0DAIkesJhtQ8SC0Gpp8yNp-6zpJdqY-hs_Q2wlAOc9IVLjdxQU2i31yLZcPE8oQTDkqMxuIo5D93ek5c8W0mziPxr0aTxIoPbGQHgKObSJ3kvwBQk6U90M14qpkKZhIPfJXXYepkshMsZTylFH-Jrv_CrAgMdYZK7_TQQd8L8DXitns7Qfr4RmF9Ug3VzxUB47MPE9p2_KhVyTq1EPR-DZ6MYmW2W71T_IjCl0sGJPnjJCgLQuTC885IPmYIUmWFP7mmjP0bfv6gexoeUFLx8BrR11jH775PfBCKz4SujjeuxnsuZflBcwmRcqT8FndSboX2l4HaV1RrtPvRbX1cP4FL8ErVjInFieQUF8K6AFCUPGBFBzT9XpkFuZg_YfCbNJ31SffgzGLSXVcc4_KePJfV6Q4esx10EhtD5Lw-N6R2hrP8D0ekvmfdufI0vUWd651uC-va9cS4iQj6ZHK9XwNTiWYsNy4MXNHsUOSjhpBBdKo0vV9eH5bMvT2BQPI6Nt2s4scl4AczPIf5wucTWchIaw7xFkk4E6Gh4JjsSxhKP95hFGpw4tsy-ffFCnC8LHvg0jMqI3PoI6r7w9WSy6TbpzjRDcjrgAb-chNRrvs_KmPQOLxF3a6NRCQDSlY3H1P180EgkXxafOwDMoBJX1-UEDM0q4bwldfXoCVKhkebSbdkJniYAQgyDSV4tIKXzT_K4krk9B5PpSuAf0Lnq5QYTESJlTR23xOtpmi5C_ys-ujHMcRrdO8BnYOqATG9gyDEKl5Tm3QROrUW7VTnY1kbqE8lG-DZnmcfOw9FwftvcleFftmWkU5QyXdzqkd50Uaji3k1xjYqIuqLefT8im4IbZ6v_Hrhn3aQ94UAC9XPAXo4I5jt62Hp_yFFuWrhhkUOMogONyiF4PfeK6u0JFCtXxW9duEY1TGjJXvaNPIbTRXJDAgPUDKWcANs8NIl49nffOwjyjPqgZAlkIywXrL60f36Arv_cgngXe1IXkMva_QEFvtXic4l2kY4brf54vm7Rqc67qS9yv9f-m8_xTry7qkmCKjEVxkzAJAoOl8ho-DzSA8j-lSooNgW0HxiUAXiuPOuHL5qnHFG6dr4o6JlGV5tjbqacgWdl391Z14rOO-VKKK19lXaXHkRUU4kaORGrWfHqlZekgWwbuKXh8_bbnBCMmd05Aoj2H22V0bcu0TUKRveleTFUstQzI80-59AyXnQsPwFcZhbh8juV0J0yguRRptgSGPa219zhVWPwnWlhf84xHMBvWCriBDYUy31aZGqRFaVLdd0iSRGyLWrfgq6w2ZU_rFA-IIFVg7zlVOC1CQF3p_d6tbxApxLJjn1UGy3p-PYveUrCcLMEaPRSzQ-ZuK4bj34rEiOQfUJfSpAXLaaEp2OVceQu_D4j0K2iZGL74-AM8qhjyjG0Tsp7Qv4GJBVR_gSRR0mEiDM_947AGY1QyGZtaIGsSzt54x8-VkFgXVsviKQfG5D1qp_JgF8_w5NGJ-I7-PiYd4uMLumGO9JdzbWJNfp3wXF09IcY87hQnFde39Zewi_8LgQ5FHRXCqAs-bJS0NK7gxSMWCtXPquRNj0pgJAMeC-hCaZggBmr7L4l0GGQmnieXQSfN6vkDlA0bFXaETk2L8SH5nECXz2u4_BUyeneJ79_yGhDaoKx4cTGvm4mr33x2V9JqRrhb0FnbmTo9aQmuVQGnrVpEV520GWQk8Uq2S1hZBl1JkZlXKNe8vLGQ4gDuVnDcIPIv2lQgG9TSbE5rEa5wGwK-xyXPgKmLUIv1AwT2vmRA7caG7xvul8AJNKnW74FJBnefncqhfLf71yQ0Ipnfl9Nj_Pj7c-rIwBNIUgt3F-nyQ0RUsHGApawQZT4HKQXazVKE40ZsWqqOGu5E3asT2fxnSEtNSQ-34ueFa6266-038wFFp9p873sKfLV6owgC9cqNgB8yMMwHaw8YhHo_I778fuxzouPixXxoJ3MeTAOJ1kqD9jxf3R2Xo_DHezRiA5WeL6ml5EAT-_8mXFQVRS9Q6FZVXF99kEtmwenwsIEFyotWUnF4ucqNiKPAWZskqIKbe9-CaO3-7cFqycBSGdZ5BvjPTQBxcw0RJSbldqW2rHlymFfanQWL3MRN9iQqsM64211MqByDXuDo7tWL2BxiMw7ls6lJeLhJEcEFnKAZvNdlDaH0fY8fOnH_YTthteyp9sgAN79qgEI4Er-1t4NqUB5cgX9jgqp0QQbRfhRwmiuNRvDJq8uyyA7l7VLBH_l2UdjvY8nJNfmcRTATx6voUvhPYRhHSKHjCGMTQ8idBfTonnXEYLio7MP6hmu2SSpvmcu1TYRd5mo8yYgPTWHwu7olwVt-42f035De7MAxX7bLC-qnGp2e6wUkdwebi40WBDRNkMEddIXY8Pv6K9Z5VwkeB8NK2ahxtw-gTO1V988ilkt1R9__Ecjnwzmx1IDv6bmpk8M28KqA91d58Fo05k78uDSLkR9fsTada3kRVGGg1erlEkfGv7q-bKT__Ysrz9AQLjUSWMSm740-9cSy-ONW05MUHwSOJyvnt2jXrl-zCeWr67p83m1A2iQRkqnWEqQ3pBISkUt8F2TCFq_sq7KEqg3EOuH0PxlMdmfrsq8DZIyJmFK1H5qD5kVITIssik5WMxnzDv1ZBfruOF0Xwd21jTkwBEwwIYu8HGh53BQKUGQCzYC2dVPeqXOm6uT2eBXhKU"
        //    });
        //}
    }
}
