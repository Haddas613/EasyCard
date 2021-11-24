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

                ecngTerminal = await SyncTerminalSettings();

                RateVat = ecngTerminal.Settings.VATRate.GetValueOrDefault() == 0 ? 0.17m : ecngTerminal.Settings.VATRate.GetValueOrDefault();

                if (isRapidOneClient)
                {
                    rapidOneItems = await SyncRapidOneItems(dataFromFile.Products);
                }

                ecngItems = await SyncECNGItems();

                return 0;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);

                return 1;
            }
        }




        private async Task<DataFromMDBFile> ReadDataFromMDBFile()
        {
            try
            {
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

                    DataFromMDBFile data = new DataFromMDBFile() {
                        BillingSetting = billingSettings.AsList()[0],
                        Products = items.AsList(),
                        NotActiveProducts = itemsnotActive.AsList(),
                        Customers = customers.AsList(),
                        ProductsPerCustomer = productsPerCustomer.AsList(),
                        RowndsProductsPerCustomer = rowndsProduct.AsList() 
                    };

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

            var updateTerminalReq = mapper.Map<UpdateTerminalRequest>(terminal);

            updateTerminalReq.Settings.VATExempt = !dataFromFile.BillingSetting.AddMaam;
            updateTerminalReq.BillingSettings.CreateRecurrentPaymentsAutomatically = false;

            //to do save vat rate todo
            //decimal RateVat = terminal.Settings.VATRate ?? 0;

            var resp = await metadataMerchantService.UpdateTerminal(updateTerminalReq);

            return await metadataMerchantService.GetTerminal(terminalRef.TerminalID);
        }

        private async Task<Dictionary<string, ItemWithPricesDto>> SyncRapidOneItems(IEnumerable<Product> products)
        {
            var allR1Items = (await rapidOneService.GetItems()).ToDictionary(d => d.Name);
            foreach (var item in products)
            {
                var foundItemsInRapid = allR1Items.TryGetValue(item.RivName, out var r1Item);
               
                if (!foundItemsInRapid)
                {
                    var newR1Item = new RapidOne.Models.ItemDto() {
                        Name = item.RivName,
                        Prices = new[] { new RapidOne.Models.ItemPriceDto() { 
                            Price = item.RivSum
                            }
                        }
                    };

                    await rapidOneService.CreateItem(newR1Item);
                }
            }

            return (await rapidOneService.GetItems()).ToDictionary(d => d.Name);
        }

        private async Task<Dictionary<string, ItemSummary>> SyncECNGItems()
        {
            var allEcngItems = (await metadataMerchantService.GetItems(new ItemsFilter { ShowDeleted = true, Origin = config.Origin })).Data.ToDictionary(d => d.ExternalReference);

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

                    var AddUtemsRes = await metadataMerchantService.CreateItem(itemRequest);
                }
            }

            return (await metadataMerchantService.GetItems(new ItemsFilter { ShowDeleted = true, Origin = config.Origin })).Data.ToDictionary(d => d.ExternalReference);
        }

        private async Task SyncECNGCustomers()
        {
            foreach (var customerInFile in dataFromFile.Customers)
            {
                var consumer = await SyncECNGCustomer(customerInFile);

                var token = await CreateTokenPerCustomer(customerInFile, consumer);

                // items pack
                var itemsPerCustomerInFile = dataFromFile.ProductsPerCustomer.Where(x => x.DealID == customerInFile.DealID);
                List<Item> items = new List<Item>();
                foreach (var itemInFile in itemsPerCustomerInFile)
                {
                    var item = metadataMerchantService.GetItems(new ItemsFilter() { BillingDesktopRefNumber = itemInFile.RivID });
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
                        ItemID = item.Result.Data.GetEnumerator().Current.ItemID,
                        NetAmount = netAmount,
                        VAT = Math.Round(netAmount * RateVat, 2, MidpointRounding.AwayFromZero),
                        VATRate = RateVat
                    });
                }

                // TODO: logging
                await CreateBillingDeal(customerInFile, token, consumer, items);
            }
        }

        private async Task<ConsumerResponse> SyncECNGCustomer(Customer customerInFile)
        {
            var externalReference = isRapidOneClient ? $"RPS_{customerInFile.RivCode}" : null; // TODO: check if consumer exist in R1
            var consumerAddress = new Shared.Integration.Models.Address() { City = customerInFile.CityID, Street = customerInFile.Street, Zip = customerInFile.ZipCode };
            var bankDetails = new Shared.Integration.Models.PaymentDetails.BankDetails() { Bank = customerInFile.BankID, BankAccount = customerInFile.BankAccount, BankBranch = customerInFile.BankBranch/*, PaymentType = Shared.Integration.Models.PaymentTypeEnum.Bank* todo to check*/ };
            var customerName = string.Format("{0} {1}", customerInFile.LastName, customerInFile.FirstName);

            ConsumersFilter cf = new ConsumersFilter
            {
                BillingDesktopRefNumber = customerInFile.DealID,
                ShowDeleted = true,
                Origin = config.Origin
            };

            Guid? consumerID = (await metadataMerchantService.GetConsumers(cf))?.Data.FirstOrDefault()?.ConsumerID;
            if (consumerID.HasValue)
            {
                var resCreateCustomer = await metadataMerchantService.UpdateConsumer(new MerchantProfileApi.Models.Billing.UpdateConsumerRequest
                {
                    Active = customerInFile.Active,
                    BankDetails = bankDetails,
                    BillingDesktopRefNumber = customerInFile.DealID,
                    ConsumerAddress = consumerAddress,
                    ConsumerName = customerName,
                    ConsumerNationalID = customerInFile.ClientCode,
                    ConsumerPhone = customerInFile.Phone1,
                    ConsumerID = consumerID.Value,
                    ConsumerSecondPhone = customerInFile.Phone2,
                    ExternalReference = externalReference,
                    Origin = config.Origin
                });
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
                    Origin = config.Origin
                });
                consumerID = resCreateCustomer.EntityUID ?? Guid.Empty;
            }

            return await metadataMerchantService.GetConsumer(consumerID.Value);

        }

        // TODO: check if token for this card already exists
        private async Task<Guid?> CreateTokenPerCustomer(Customer customerInFile, ConsumerResponse ecngCustomer)
        {
            if (!String.IsNullOrEmpty(customerInFile.CardNumber))
            {
                var request = new Transactions.Api.Models.Tokens.TokenRequest()
                {
                    ConsumerID = ecngCustomer.ConsumerID,
                    CardNumber = customerInFile.CardNumber,
                    CardExpiration = ConvertCardDateToMonthYearcs.GetMonthYearFromCardDate(customerInFile.CardDate),
                    CardOwnerName = string.Format("{0} {1}", customerInFile.LastName, customerInFile.FirstName),
                };

                return (await transactionsService.CreateToken(request)).EntityUID;
            }

            return null;
        }


        private async Task CreateBillingDeal(Customer customerInFile, Guid? TokenCreditCard, ConsumerResponse consumer, List<Item> items)
        {
            var request = new Transactions.Api.Models.Billing.BillingDealRequest()
            {
                BankDetails = new Shared.Integration.Models.PaymentDetails.BankDetails() { Bank = customerInFile.BankID, BankAccount = customerInFile.BankAccount, BankBranch = customerInFile.BankBranch },
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
                PaymentType = Models.Helper.EnumConvertor.ConvertToPaymentType(customerInFile.PayType),
                TransactionAmount = customerInFile.TotalSum,

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
                }
            };

            var AddBillingDeal = await transactionsService.CreateBillingDeal(request);
        }
    }
}
