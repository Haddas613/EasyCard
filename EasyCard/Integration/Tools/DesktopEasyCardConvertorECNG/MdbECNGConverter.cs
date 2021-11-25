using AutoMapper;
using Common.Models;
using Common.Models.MDBSource;
using Dapper;
using DesktopEasyCardConvertorECNG.Models;
using DesktopEasyCardConvertorECNG.Models.Helper;
using DesktopEasyCardConvertorECNG.RapidOneClient;
using MerchantProfileApi.Client;
using MerchantProfileApi.Models.Billing;
using MerchantProfileApi.Models.Terminal;
using Microsoft.Extensions.Logging;
using RapidOne;
using RapidOne.Models;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Api.Client;

namespace DesktopEasyCardConvertorECNG
{
    public class MdbECNGConverter
    {
        private readonly ILogger logger;
        private readonly AppConfig config;
        private readonly IMapper mapper;

        private readonly RapidOneService rapidOneService;
        private readonly TransactionsApiClient transactionsService;
        private readonly MerchantMetadataApiClient metadataMerchantService;

        private readonly bool isRapidOneClient = false;


        private TerminalResponse ecngTerminal;
        private Dictionary<string, ItemWithPricesDto> rapidOneItems;
        private DataFromMDBFile dataFromFile;
        private Dictionary<string, ItemSummary> ecngItems;
        private decimal RateVat = 0;
        private ItemCategoryDto defaultRapidItemCategory;

        public MdbECNGConverter(ILogger logger, AppConfig config, IMapper mapper, RapidOneService rapidOneService, TransactionsApiClient transactionsService, MerchantMetadataApiClient metadataMerchantService)
        {
            if (rapidOneService != null)
            {
                isRapidOneClient = true;
                this.rapidOneService = rapidOneService;
            }

            this.logger = logger;
            this.config = config;
            this.mapper = mapper;
            this.rapidOneService = rapidOneService;
            this.transactionsService = transactionsService;
            this.metadataMerchantService = metadataMerchantService;

        }

        public async Task<int> ProcessMdbFile()
        {
            try
            {
                dataFromFile = await ReadDataFromMDBFile();

                logger.LogInformation($"----------------------------------");

                ecngTerminal = await SyncTerminalSettings();

                logger.LogInformation($"----------------------------------");

                RateVat = ecngTerminal.Settings.VATRate.GetValueOrDefault();

                if (isRapidOneClient)
                {
                    defaultRapidItemCategory = await SyncDefaultRapidItemCategory();

                    logger.LogInformation($"----------------------------------");

                    rapidOneItems = await SyncRapidOneItems(dataFromFile.Products);

                    logger.LogInformation($"----------------------------------");
                }

                ecngItems = await SyncECNGItems();

                logger.LogInformation($"----------------------------------");

                await SyncECNGCustomers();

                logger.LogInformation($"END");

                return 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                return 1;
            }
        }




        private async Task<DataFromMDBFile> ReadDataFromMDBFile()
        {
            try
            {
                logger.LogInformation($"Loading from {config.FullPathToMDBFile}");

                string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={config.FullPathToMDBFile};Jet OLEDB:Database Password=D09h3c;";
                using (OleDbConnection myConnection = new OleDbConnection(connectionString))
                {
                    myConnection.Open();

                    var billingSettings = await myConnection.QueryAsync<BillingSettings>("SELECT * FROM tblOptions");

                    var items = await myConnection.QueryAsync<Product>("SELECT * FROM tblRivName");

                    var itemsnotActive = await myConnection.QueryAsync<NotActiveProduct>("SELECT CusProd.DealText as ProductName,DealSum/DealCount as ProdSum,'notActive',CusProd.rivid as DesktopRecordID,ProdTab.RivCode as RivCoded from tblDealProp   as CusProd inner join tblrivName  as ProdTab on  CusProd.rivid = ProdTab.revid where    ProdTab.rivName<> CusProd.DealText");

                    var customers = await myConnection.QueryAsync<Customer>("SELECT * FROM tblDeal");

                    var productsPerCustomer = await myConnection.QueryAsync<ProductPerCustomer>(" SELECT DealID, DealText, DealSum/ DealCount as ProdSum,DealCount,RivID,products.RivCode FROM tblDealProp as prodpercust inner join tblrivname as products on    products.revid = prodpercust.rivid");

                    var rowndsProduct = await myConnection.QueryAsync<RowndsProductsPerCustomer>("select customers.dealid as DealID, customers.totalsum, sums.customerTotalSum, sums.customerTotalSum * 1.17 as sumprodWithMaam from tblDeal as customers inner join(select DEALID, SUM(DealSum) as customerTotalSum from tbldealPROP GROUP BY DEALID) as sums on sums.dealid = customers.dealid");

                    DataFromMDBFile data = new DataFromMDBFile()
                    {
                        BillingSetting = billingSettings.AsList()[0],
                        Products = items.AsList(),
                        NotActiveProducts = itemsnotActive.AsList(),
                        Customers = customers.AsList(),
                        ProductsPerCustomer = productsPerCustomer.AsList(),
                        RowndsProductsPerCustomer = rowndsProduct.AsList()
                    };

                    logger.LogInformation($"Loaded from MDB:");
                    logger.LogInformation($"\tBillingSetting");
                    logger.LogInformation($"\tProducts: {data.Products.Count()}");
                    logger.LogInformation($"\tNotActiveProducts: {data.NotActiveProducts.Count()}");
                    logger.LogInformation($"\tCustomers: {data.Customers.Count()}");
                    logger.LogInformation($"\tProductsPerCustomer: {data.ProductsPerCustomer.Count()}");
                    logger.LogInformation($"\tRowndsProductsPerCustomer: {data.RowndsProductsPerCustomer.Count()}");

                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OLEDB Connection FAILED: " + ex.Message);
                throw;
            }
        }

        private async Task<TerminalResponse> SyncTerminalSettings()
        {
            var terminalRef = (await metadataMerchantService.GetTerminals()).Data?.FirstOrDefault();
            if (terminalRef == null)
            {
                throw new Exception("Cannot get ECNG terminal");
            }

            var terminal = await metadataMerchantService.GetTerminal(terminalRef.TerminalID);

            logger.LogInformation($"Loaded Terminal data: {terminal.Label} ({terminal.TerminalID})");

            var updateTerminalReq = mapper.Map<UpdateTerminalRequest>(terminal);

            updateTerminalReq.Settings.VATExempt = !dataFromFile.BillingSetting.AddMaam;
            updateTerminalReq.BillingSettings.CreateRecurrentPaymentsAutomatically = false;

            //to do save vat rate todo
            //decimal RateVat = terminal.Settings.VATRate ?? 0;

            var resp = await metadataMerchantService.UpdateTerminal(updateTerminalReq);

            logger.LogInformation($"Updated Terminal data: {resp.Status}");

            var res = await metadataMerchantService.GetTerminal(terminalRef.TerminalID);

            logger.LogInformation($"\tVATExempt: {res.Settings.VATExempt}");
            logger.LogInformation($"\tVATRate: {res.Settings.VATRate}");
            logger.LogInformation($"\tDollarRate: {res.Settings.DollarRate}");
            logger.LogInformation($"\tEuroRate: {res.Settings.EuroRate}");
            logger.LogInformation($"\tDoNotCreateSaveTokenInitialDeal: {res.Settings.DoNotCreateSaveTokenInitialDeal}");
            logger.LogInformation($"\tCvvRequired: {res.Settings.CvvRequired}");
            logger.LogInformation($"\tCreateRecurrentPaymentsAutomatically: {res.BillingSettings.CreateRecurrentPaymentsAutomatically}");

            return res;
        }

        private async Task<ItemCategoryDto> SyncDefaultRapidItemCategory()
        {
            var itemCategory = (await rapidOneService.GetItemCategories())?.FirstOrDefault(d => d.Name == config.RapidItemCategoryName);

            if (itemCategory == null)
            {
                itemCategory = new ItemCategoryDto
                {
                    Active = 1,
                    Name = config.RapidItemCategoryName
                };

                itemCategory = (await rapidOneService.CreateItemCategory(itemCategory)); // TODO: process error case

                logger.LogInformation($"Created RapidOne default Item Category: {itemCategory.Name} ({itemCategory.Code})");
            }
            else
            {
                logger.LogInformation($"RapidOne default Item Category: {itemCategory.Name} ({itemCategory.Code})");
            }

            return itemCategory;
        }

        private async Task<Dictionary<string, ItemWithPricesDto>> SyncRapidOneItems(IEnumerable<Product> products)
        {
            try
            {
                var srvItems = (await rapidOneService.GetItems());
                var allR1Items = srvItems.GroupBy(d => d.Name).Select(x => x.FirstOrDefault()).ToDictionary(d => d.Name);

                logger.LogInformation($"Loaded RapidOne Items: {allR1Items.Count} ({srvItems.Count()})");

                foreach (var item in products)
                {
                    var foundItemsInRapid = allR1Items.TryGetValue(item.RivName, out var r1Item);

                    if (!foundItemsInRapid)
                    {
                        var newR1Item = new RapidOne.Models.ItemDto()
                        {
                            Name = item.RivName,
                            Active = 1,
                            ManufacturerCode = -1,
                            CategoryCode = (defaultRapidItemCategory?.Code).GetValueOrDefault(),
                            Prices = new[] { new RapidOne.Models.ItemPriceDto() {
                            Price = item.RivSum
                            }
                        }
                        };

                        var itemRes = await rapidOneService.CreateItem(newR1Item);

                        logger.LogInformation($"Created RapidOne Item: {itemRes.Name} ({itemRes.Code})");
                    }
                    else
                    {
                        logger.LogInformation($"Found RapidOne Item: {r1Item.Name} ({r1Item.Code})");
                    }
                }

                return (await rapidOneService.GetItems()).GroupBy(d => d.Name).Select(x => x.FirstOrDefault()).ToDictionary(d => d.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to sync RapidOne items");
                return null;
            }

        }

        private async Task<Dictionary<string, ItemSummary>> SyncECNGItems()
        {
            var allEcngSrvItems = (await metadataMerchantService.GetItems(new ItemsFilter { ShowDeleted = Shared.Helpers.Models.ShowDeletedEnum.All, Origin = config.Origin })).Data;
            var allEcngItems = allEcngSrvItems.Where(d => !string.IsNullOrWhiteSpace(d.BillingDesktopRefNumber)).GroupBy(d => d.BillingDesktopRefNumber).Select(x => x.FirstOrDefault()).ToDictionary(d => d.BillingDesktopRefNumber);

            logger.LogInformation($"Loaded ECNG Items: {allEcngItems.Count} ({allEcngSrvItems.Count()})");

            foreach (var product in dataFromFile.Products)
            {
                if (!allEcngItems.TryGetValue(product.RevID, out var ecngItem))
                {
                    ItemWithPricesDto r1Item = null;

                    var foundItemsInRapid = isRapidOneClient ? rapidOneItems.TryGetValue(product.RivName, out r1Item) : false;

                    var itemRequest = new ItemRequest()
                    {
                        Active = true,
                        BillingDesktopRefNumber = product.RevID,
                        ItemName = product.RivName,
                        Price = product.RivSum,
                        ExternalReference = r1Item?.Code,
                        Origin = config.Origin
                        // Currency = product. no currency in this lever in Desktop
                        // SKU = //product.RivCodeקופת הכנסה לא קוד
                    };

                    var addItemsRes = await metadataMerchantService.CreateItem(itemRequest);

                    logger.LogInformation($"Created ECNG item : {product.RivName} ({addItemsRes.EntityUID} - {product.RevID})");
                }
                else
                {
                    logger.LogInformation($"Found ECNG item : {ecngItem.ItemName} ({ecngItem.ItemID} - {product.RevID})");
                }
            }

            return (await metadataMerchantService.GetItems(new ItemsFilter { ShowDeleted = Shared.Helpers.Models.ShowDeletedEnum.All, Origin = config.Origin })).Data.Where(d => string.IsNullOrWhiteSpace(d.BillingDesktopRefNumber)).GroupBy(d => d.BillingDesktopRefNumber).Select(x => x.FirstOrDefault()).ToDictionary(d => d.BillingDesktopRefNumber);
        }

        private async Task SyncECNGCustomers()
        {
            foreach (var customerInFile in dataFromFile.Customers)
            {
                try
                {
                    var consumer = await SyncECNGCustomer(customerInFile);

                    var token = await CreateTokenPerCustomer(customerInFile, consumer);

                    // items pack
                    var itemsPerCustomerInFile = dataFromFile.ProductsPerCustomer.Where(x => x.DealID == customerInFile.DealID);
                    List<Item> items = new List<Item>();
                    foreach (var itemInFile in itemsPerCustomerInFile)
                    {
                        var ecngItemExists = ecngItems.TryGetValue(itemInFile.RivID, out var item);

                        decimal amount = Math.Round(itemInFile.ProdSum * itemInFile.DealCount, 2, MidpointRounding.AwayFromZero);
                        decimal netAmount = dataFromFile.BillingSetting.AddMaam ? amount : Math.Round(((amount) / 1m + RateVat), 2, MidpointRounding.AwayFromZero);
                        items.Add(new Item()
                        {
                            Price = itemInFile.ProdSum,
                            ExternalReference = itemInFile.RivID,
                            ItemName = itemInFile.DealText,
                            Quantity = itemInFile.DealCount,
                            //   SKU = itemInFile.RivCode,/* from rapid ifit's rapid todo to do ,*/
                            Amount = amount,
                            ItemID = item?.ItemID,
                            NetAmount = netAmount,
                            VAT = Math.Round(netAmount * RateVat, 2, MidpointRounding.AwayFromZero),
                            VATRate = RateVat
                        });
                    }

                    // TODO: logging
                    await CreateBillingDeal(customerInFile, token, consumer, items);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to sync customer {customerInFile.DealID} {ex.Message}");

                    if (ex is WebApiServerErrorException)
                    {
                        logger.LogError(((WebApiServerErrorException)ex).Response);
                    }
                    else if (ex is WebApiClientErrorException)
                    {
                        logger.LogError(((WebApiClientErrorException)ex).Response);
                    }
                }
            }
        }

        private async Task<ConsumerResponse> SyncECNGCustomer(Customer customerInFile)
        {
            var externalReference = string.IsNullOrWhiteSpace(customerInFile.RivCode) ? null : isRapidOneClient ? $"RPS_{customerInFile.RivCode}" : null; // TODO: check if consumer exist in R1
            var consumerAddress = new Shared.Integration.Models.Address() { City = customerInFile.CityID, Street = customerInFile.Street, Zip = customerInFile.ZipCode };

            var customerName = string.Format("{0} {1}", customerInFile.LastName, customerInFile.FirstName);

            Shared.Integration.Models.PaymentDetails.BankDetails bankDetails = null;
            if (customerInFile.BankID > 0 && !string.IsNullOrWhiteSpace(customerInFile.BankAccount) && customerInFile.BankBranch > 0)
            {
                bankDetails = new Shared.Integration.Models.PaymentDetails.BankDetails() { Bank = customerInFile.BankID, BankAccount = customerInFile.BankAccount, BankBranch = customerInFile.BankBranch/*, PaymentType = Shared.Integration.Models.PaymentTypeEnum.Bank* todo to check*/ };
            }

            ConsumersFilter cf = new ConsumersFilter
            {
                BillingDesktopRefNumber = customerInFile.DealID,
                ShowDeleted = Shared.Helpers.Models.ShowDeletedEnum.All,
                Origin = config.Origin
            };

            Guid? consumerID = (await metadataMerchantService.GetConsumers(cf))?.Data.FirstOrDefault()?.ConsumerID;
            if (consumerID.HasValue)
            {
                var existingConsumer = await metadataMerchantService.GetConsumer(consumerID.Value);

                var request = mapper.Map<UpdateConsumerRequest>(existingConsumer);

                request.Active = customerInFile.Active;
                request.BankDetails = bankDetails;
                request.ConsumerAddress = consumerAddress;
                request.ConsumerName = customerName;
                request.ConsumerNationalID = customerInFile.ClientCode;
                request.ConsumerPhone = customerInFile.Phone1;
                request.ConsumerSecondPhone = customerInFile.Phone2;
                request.ExternalReference = externalReference;

                var resUpdateCustomer = await metadataMerchantService.UpdateConsumer(request);

                logger.LogInformation($"Updated ECNG customer {customerName} ({consumerID} - {externalReference})");
            }
            else
            {
                var resCreateCustomer = await metadataMerchantService.CreateConsumer(new MerchantProfileApi.Models.Billing.ConsumerRequest
                {
                    Active = customerInFile.Active,
                    BankDetails = bankDetails,
                    BillingDesktopRefNumber = customerInFile.DealID,
                    ConsumerAddress = consumerAddress,
                    ConsumerName = customerName,
                    ConsumerNationalID = customerInFile.ClientCode,
                    ConsumerPhone = customerInFile.Phone1,
                    ConsumerSecondPhone = customerInFile.Phone2,
                    ExternalReference = externalReference,
                    Origin = config.Origin,
                    TerminalID = ecngTerminal.TerminalID
                });
                consumerID = resCreateCustomer.EntityUID ?? Guid.Empty;

                logger.LogInformation($"Created ECNG customer {customerName} ({consumerID} - {externalReference})");
            }

            return await metadataMerchantService.GetConsumer(consumerID.Value);

        }

        // TODO: do not create token for expired card
        private async Task<Guid?> CreateTokenPerCustomer(Customer customerInFile, ConsumerResponse ecngCustomer)
        {
            var fltr = new Transactions.Api.Models.Tokens.CreditCardTokenFilter
            {
                ConsumerID = ecngCustomer.ConsumerID,
                CardNumber = CreditCardHelpers.GetCardDigits(customerInFile.CardNumber)
            };

            var existingToken = (await transactionsService.GetTokens(fltr))?.Data?.FirstOrDefault();

            if (existingToken != null)
            {
                logger.LogInformation($"Token already exist {fltr.CardNumber} ({existingToken.CreditCardTokenID}) for {ecngCustomer.ConsumerName} ({ecngCustomer.ConsumerID})");

                return existingToken.CreditCardTokenID;
            }

            if (!String.IsNullOrEmpty(customerInFile.CardNumber))
            {
                var expiration = ConvertCardDateToMonthYearcs.GetMonthYearFromCardDate(customerInFile.CardDate);

                if (expiration.Expired)
                {
                    logger.LogError($"Card expired {fltr.CardNumber} for { ecngCustomer.ConsumerName} ({ ecngCustomer.ConsumerID})");
                }

                var request = new Transactions.Api.Models.Tokens.TokenRequest()
                {
                    TerminalID = ecngTerminal.TerminalID,
                    ConsumerID = ecngCustomer.ConsumerID,
                    CardNumber = customerInFile.CardNumber,
                    CardExpiration = expiration,
                    CardOwnerName = string.Format("{0} {1}", customerInFile.LastName, customerInFile.FirstName),
                };

                var res = (await transactionsService.CreateToken(request)).EntityUID;

                logger.LogInformation($"Created token {fltr.CardNumber} ({res}) for {request.CardOwnerName} ({ecngCustomer.ConsumerID})");

                return res;
            }

            return null;
        }

        // TODO: update existing billing
        private async Task CreateBillingDeal(Customer customerInFile, Guid? TokenCreditCard, ConsumerResponse consumer, List<Item> items)
        {
            if (!(customerInFile.TotalSum > 0))
            {
                logger.LogInformation($"Skipped creating billing for {customerInFile.DealID} because amount is 0");
                return;
            }

            var paymentType = Models.Helper.EnumConvertor.ConvertToPaymentType(customerInFile.PayType);

            if (paymentType == PaymentTypeEnum.Card && !TokenCreditCard.HasValue)
            {
                logger.LogError($"Skipped creating billing for {customerInFile.DealID} because credit card token is empty");
                return;
            }

            if (paymentType != PaymentTypeEnum.Card)
            {
                logger.LogError($"Skipped TODO: payment type {paymentType} billing for {customerInFile.DealID}");
                return;
            }

            var existingBilling = (await transactionsService.GetBillingDeals(new Transactions.Api.Models.Billing.BillingDealsFilter 
            { 
                 PaymentType = paymentType,
                 CreditCardTokenID = TokenCreditCard,
                 TerminalID = ecngTerminal.TerminalID,
                 Origin = config.Origin,
                 DealReference = customerInFile.DealID
            }))?.Data?.FirstOrDefault();

            if (existingBilling != null)
            {
                logger.LogInformation($"Skipped creating billing for {customerInFile.DealID} because it is exist already");
                return;
            }

            var request = new Transactions.Api.Models.Billing.BillingDealRequest()
            {
                // TODO: bank billings
                //BankDetails = new Shared.Integration.Models.PaymentDetails.BankDetails() { Bank = customerInFile.BankID, BankAccount = customerInFile.BankAccount, BankBranch = customerInFile.BankBranch },
                BillingSchedule = new Transactions.Shared.Models.BillingSchedule()
                {
                    RepeatPeriodType = Models.Helper.EnumConvertor.ConvertToBillingType(customerInFile.BillingTypeID),
                    StartAt = CalculateDate.GetStartPayDate(customerInFile.BillingDay, customerInFile.StartDate ?? DateTime.Today),
                    EndAt = customerInFile.finishDate,
                    EndAtType = customerInFile.finishDate == null ? Transactions.Shared.Enums.EndAtTypeEnum.Never : Transactions.Shared.Enums.EndAtTypeEnum.SpecifiedDate,
                    StartAtType = Transactions.Shared.Enums.StartAtTypeEnum.SpecifiedDate,
                },
                CreditCardToken = TokenCreditCard,
                Currency = Models.Helper.EnumConvertor.ConvertToCurrecy(customerInFile.MTypeID),
                //InvoiceDetails = new Shared.Integration.Models.Invoicing.InvoiceDetails() { }
                PaymentType = paymentType,
                TransactionAmount = customerInFile.TotalSum,
                TerminalID = ecngTerminal.TerminalID,
                DealDetails = new Shared.Integration.Models.DealDetails()
                {
                    ConsumerAddress = consumer.ConsumerAddress,
                    ConsumerEmail = consumer.ConsumerEmail,
                    ConsumerExternalReference = consumer.ExternalReference,
                    ConsumerID = consumer.ConsumerID,
                    ConsumerName = consumer.ConsumerName,
                    ConsumerNationalID = consumer.ConsumerNationalID,
                    ConsumerPhone = consumer.ConsumerPhone,
                    Items = items,

                    DealDescription = "Export from MDB", // TODO
                    DealReference = customerInFile.DealID
                },
                Origin = config.Origin
            };

            var res = await transactionsService.CreateBillingDeal(request);

            logger.LogInformation($"Created billing {res.EntityUID} for {request.DealDetails.ConsumerName} ({request.DealDetails.ConsumerID}) {customerInFile.TotalSum} {request.Currency}");
   
        }
    }
}
