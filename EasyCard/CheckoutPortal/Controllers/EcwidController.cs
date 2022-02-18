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

            if (customerPayload != null)
            {
                var exisingConsumers = await merchantMetadataApiClient.GetConsumers(new ConsumersFilter
                {
                    ExternalReference = customerPayload.ExternalReference,
                    Origin = customerPayload.Origin,
                });

                if (exisingConsumers.NumberOfRecords == 1)
                {
                    consumerID = exisingConsumers.Data.First().ConsumerID;
                }
                else if (exisingConsumers.NumberOfRecords > 1)
                {
                    throw new ApplicationException("There are several consumers with same card code in ECNG");
                }

                if (consumerID == null)
                {
                    var createConsumerResponse = await merchantMetadataApiClient.CreateConsumer(customerPayload);
                    consumerID = createConsumerResponse.EntityUID;
                }
            }

            var paymentRequestPayload = ecwidPayload.GetCreatePaymentRequest();
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
        //        Data = "yFwuke-T5vcJPtMwqWODoGHMB--0uFaVKKbsG672rJm6BtCsWSyupRsfmMF2T10uq4Xq02B2zVXSXuBpmnPlMatJcR5lZo5YT4LZ6pehsA4HRFWHYnQolmy4vlck_06ry5jOSEiSJLRnDmo2gnfSrJcFL2WtxfDLsT6U-tF-ji-sMLpwqOFg2UQQdk3Yl0non3vkVXg7M450U-yIslg3sv5UUZvSV7jzol-g2zoy-KTMU4rXz7OEy4bIdvyk3SSdhVg4MEQ6kGZriyi-I4BZwD5bbCRzIbUyFUfCn4VBSHz6WFXo3Hl_XAi8_uvxoqDj0vsyT6IW5xx3Jbc3A7zCQCm2ArYjMzmovAqUOJw6w-aTCJDtNJ3eXqE2YM42jOqN4PZHzY1EIsgvrrJtG0INX_13e22PREh1X8JFy8S7IAehs771ajSWINRBvgXrYRS6-LlAa59Rn8j-1Mq81t3p5IAo6ifmEMvrWmQLLySpQRHlCV5JmIgoqoXeSneVjAnOtWh5TFE0Z-W8QW1fIFk699KgsjX3rypUC41A7-a7oGOlFPK0a9KzBXzuloMAmk2mNcgTIiIGCKEnwyYnDu4oiyu57ZmvYXTMzjLlcFC--swZy8GpYRoZK1Yxgx8YwPAj7bNx5LxVLQ1Wvi1Aba6u9qteJ8_PRGHF3Fn3Cegk91eSUJC-l-vea_yQsDMI5Y0wXeClC9G4rx8MV3ytx8T0TDTvU5xlvIexzIXOdtLVjdbQdv456ojW1GpRd7V5aPsg97xF7Z_-HF33rczUwWOMkKN6XlY7n3WfwRLRaY_THp6F1qg5mUovd7WPeSbjA7KMJWa5o_ro3wWiZWJ-vQJlApa7KYlbJwxQGjiDz6Is6njoDRNcSymT3FPqyjNQ6J9hAWe98w5zkwRvCmajE-VrMZaFDqIEM1Aqe3OFrFj33U_-a5B6BxmzqohdCK0tH9jn191yAfVX90KWIznr7ZIRuWk7zwuE__OfmnIr1TZWvZ-isyhzk9KiMPpTElJNClKKlaWGxH8xzalioZXAGVwJNWoChHtZPKQKtbUH-7Z8ZMH-cBbM_hNf40aS8HX3k5sjblYW_xvUhrykUepwnHofFovV5R2tJODDRao2NnHcHjxyIyy748GweyvNIDz_Fel_RWWCPSKzzVHxktF2rB7cc8-vi8xF3Q3flO9pyONi9dJlW8QjEMu6Pd6iJ04_T17Q5Wo8jsS0T3eEMBZDonfLRoYAbO6mDcu4XIuECpC-EzdfP6ldxlAV_Ydwyxutf39ggyfVUInpRw8VOsZ-2l10BwwnETKkM-42cyvxP9FD1SKhsWPqX59DznWgSbASCHQqRMg0omh3NH3CDgR6p3KF03IVdgN-hpdqv9kZn0qJOHKmFukYXlLl-rSMgrNoMumskHjyZI4bd8clHbI5zTtxZntdVX_tp8WAN03qWBuKVyPkzluMD9TAJFa2xoTav0haWR38pLRhHg0VxEeKJ1I7XozX50casBuf36p7GNFjE7usEH2sgRq0FrA_lLLgCGW-Pd8rx_CFV54PM5cwnGJs1QyX7OcR-T3vbwnXR9zczuUaheslhLd7NR9jasa9r_IcP5B1vs44xDE1-tcNoYrWA1_dCcVL2A3PCd02KeznsHvDaXVhEU8QCvgaGKGjAmtRPVHEv3kwTzW4B8zxdMqguC0yXu7MVe5ljYtuXTRw40QZclCheIUXduF6X4cCPKXRRNTDP-Y5OWNqPGndffIQCZOi3kjiiMKT1sSxPfsJM0qZdvKsZWt9a1OyG32pO2_BOcE-syI0qay72TjYu2zz65z1iZAfjv9MZxE6kIuEUnpTrhkUy3FEJFSPwY-jnj0ZDMfq-D9baC51-fgMA2MoF4qQ5QN2vq6PtITz7rkjw1KyDiUQzQYg5R9qe0XxaRfDaJ9lvmBrCdlSY9-w5CkpikwUobiN4x2hF7W6D8SocTi7DuI4Di9X6I1VCJwSg91jOnbQ7_4L0hvaw0z3mRr9lN9g9t8dsUIluEQh5gNn4GXIl3hbNpeCOYeoiw8zAvtjyT2HqZMa3MGyGBU_e2OYO3YZ49k9vzqrTJEHD4t3snLvWqC-Vs1UZ1NeBZZ230RVChDwPa8QgIOhcUPFgLY6w7aNPVz41DVH_dzCLDl6Vmi4T8ulHkbKY9uLamVor7P0cgjS9xCeAuR6gCS_wpni5S301owwu-wsaAecl2n5Nnlh_AM-AQ7jx-cVSqBo6L61lLaXuQniCOe7Vre4Rdt-lUtHuTfjgv5N5FZmizxAUY7JHh4d86bNBMhyPH9fhL1D3esYeAWqgQpJ7JF2u9ZIwe81QUqAbBIbCeuYRgQX3tDhv58PY2CGg78lkw1uP4oHxgCZ-dpeJZkM-uNEbhggkD5uI-_2Vp74aYC6ZWbuxTnuQ32M_IpnCSsa3N89uwWO62fKrO-apojT-LegO0fsiLCjupTisClUw09a4TqK_Gs2PCHI_bp0tvnPtv5X-8EMFUqvjRC8FT4H95Hi7Up_M7ePgrBlaKbHDb0tvcru9Ya62FWsjYCLbuGAmPyHBb8BjV6xwKiHjJkbZ_ZKz9WpsN9coaEwo3_TNme9ZcsFSZ47D12b-aOQUlRtZrA2xUD7ahrD06jAbmmBqTMgCPvcAJcymgmoEPYJlLovYr32wNW2Sh1dq8dUwIkDjRqpj-jq8CNPjrkNoF32YL-Uq-2VfexCD9Axd4XVEQjHwa_mz2-P-wdb9uD5cP2lIdnruvlcp61XwRIL_ewSR6bp48mX6E2jNVe_H-kRdUlZXtMoIetiuAX5zyF58HuTnjAIapXgJcrgso2OPpxdeu5ECmWYs7sG1rW0T5sKlWfSJyAkv_mqpFz7yVLSoOzoaKCzO-tAhcvEGZrOdWNHYf41-efqyOZZGF6wVTykyUZHUsn723heJhCGNuVGW5PYmFwPB0JpFxEwdR_1ERzjX3JP8rJ3-TlHkFv8JhvAY1APZXdEnizE_oK88ivd3ljjBHwsHaOfhXSrh6bpzeno4YGfiBIZ3oTUwk-bWdm62zJI8H67lO7n3i9bb5MSkeOLVz2YU9i2dg5aoM28cTsxyhE1nk1Co3DuM43Y8xaUVgLn8C02iPF05_StjEbQTx2oMSkYfdI1bXJRL4qeaD91uKS1AJtUVx3tIKY6fImy4mkLKsMDa3KCNphuyPkK-QP1hADBNSmHzZFJB3Iri__k5qnKBMXFomUSbSE2t_0CM-PAFnHLsW9PyVwPmPGeE40-iTEUg9KvWWmpCfLeD4e_j1aJlltp2E1qHJJY62TRxGb_K51LooBvT8XINgs8dIeue1-1ndh4YhEAj95SY1uYqFlGbvMvNRbFnsks71dah8OOZ-NDu8UErQQH0lobPAw8hlPX0LzUVrjeUpMPFHJUk3xk0mubJnUsyPAGPVtVXc70WZaZ7QBN5GJh4XFMHwjJCyhOYknLuqnE5F3MtcdCehLe6rJ7Lm-qoTBR9bj3w7b_xLhyqrKpQhWEfblt1W89KoZ8FvCOz5HejijExXUfzvF1L-L0knZmXIFOn_3p208PBRN19H76PCR2BFA4eMTC_6Fp7QmyPwgQJnbdDsqHkJAkie5C0yU4jG_NWz69-sAWSV_KwdKJHTEh65NZo10W3wmP1nglH1LA7RRgihMRs5GFZuzwwtU88zj8Qm3zoFZM_yXgd-lX_I6LieW0Rln1ANuvMMabUuN4IYB2KUexVuYCKcAk_WqsBJXAfXk5kpbMItVP4xGJKllJbPJWKL0tuQ5AJAFgESkA2ptllBHUxdF05imGVIEaIIszwvrNf5S3vb-bvAhPFLVXuzUJ6xC33l0BL3parwJMCPsDns4pI2USNSFaXz3iQ_PEhiQ3x01oVBF1-hultGOIl6B0GvzUGQ-KtWc82kKh68lln0Yv_FmhIn0bw5BLajr8y8qaNMp5GNTNkWhvazj8Ubp4GpLzSyosw9QZGGm367mAKu6muT_Ip_WQq-lEoisP6CiepS57m855MP111Oew34V2AlJG-t45ai17K6DgnkVx6RHgRh8Ycwv6ptRU3jCBnISk4J8vUImjvUihGnlWkJ2KfJpNXF9U3KXzSQLPBY_P_S_jeahgOL0AosIBGessEfDScP1J3TkBK9dMUn4PrFA8xKu-oOPJlOUwbsZCFJyCuwSAbNG8Y975-26iWLmazDcg_r5pGGf8wH_9g4Dk16WZymUMQUXSiWHl"
        //    });
        //}
    }
}
