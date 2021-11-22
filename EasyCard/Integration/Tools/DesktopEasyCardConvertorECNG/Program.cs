using Common.Models.MDBSource;
using DesktopEasyCardConvertorECNG.Models.Helper;
using MerchantProfileApi.Client;
using MerchantProfileApi.Models.Billing;
using Shared.Api.Models;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transactions.Api.Client;
using Transactions.Api.Models.Tokens;

namespace DesktopEasyCardConvertorECNG
{
    class Program
    {
        static void Main(string[] args)
        {
            bool RapidClient = false;
            string MerchantName = "Test";



            Console.WriteLine("Enter true if the client is Rapid Client or false if not:");
            string RapidClientStr = Console.ReadLine();
            while (!Boolean.TryParse(RapidClientStr, out RapidClient))
            {
                Console.WriteLine("Error typing, Please type true or false");
                RapidClientStr = Console.ReadLine();
            }

            var dataFromFile = ReadMDBFile.ReadDataFromMDBFile(null).Result;
            var serviceFactory = new ServiceFactory(args[0], Environment.QA);

            var metadataMerchantService = serviceFactory.GetMerchantMetadataApiClient();
            var metadataTerminalService = serviceFactory.GetTransactionsApiClient();
            foreach (var product in dataFromFile.Products)
            {
                var AddUtemsRes = metadataMerchantService.CreateItem(new ItemRequest()
                {
                    Active = true,
                    BillingDesktopRefNumber = product.RevID,
                    ItemName = product.RivName,
                    Price = product.RivSum,
                    ExternalReference = product.RevID
                });
            }

            foreach (var customerInFile in dataFromFile.Customers)
            {
                Guid customerID = ImportCustomer(RapidClient, MerchantName, metadataMerchantService, customerInFile);
                var findcustomer = metadataMerchantService.GetConsumers(new ConsumersFilter() { BillingDesktopRefNumber = customerInFile.DealID });
                var token = CreateTokenPerCustomer(customerInFile.CardNumber, metadataTerminalService, customerID, customerInFile);
                var itemsPerCustomerInFile = dataFromFile.ProductsPerCustomer.FindAll(x => x.DealID == customerInFile.DealID);
                List<Item> items = new List<Item>();
                foreach (var itemInFile in itemsPerCustomerInFile)
                {
                    items.Add(new Item() { Price = itemInFile.ProdSum, ExternalReference = itemInFile.RivID, ItemName = itemInFile.DealText, Quantity = itemInFile.DealCount /*SKU = itemInFile.RivID todo to do */});
                }
                CreateBillingDeal(metadataTerminalService, customerInFile, token.Result.EntityUID??Guid.Empty, findcustomer.Result, items);
            }

            Console.WriteLine("success"/*res.Customers?.Count*/);
        }

        private static Guid ImportCustomer(bool RapidClient, string MerchantName, MerchantMetadataApiClient metadataMerchantService, Customer customerInFile)
        {
            ConsumersFilter cf = new ConsumersFilter();
            cf.BillingDesktopRefNumber = customerInFile.DealID;
            var findcustomer = metadataMerchantService.GetConsumers(cf);
            Guid consumerID;
            if (findcustomer.Result.NumberOfRecords > 0)
            {
                consumerID = findcustomer.Result.Data.GetEnumerator().Current.ConsumerID;
                var resCreateCustomer = metadataMerchantService.UpdateConsumer(new MerchantProfileApi.Models.Billing.UpdateConsumerRequest
                {
                    Active = customerInFile.Active,
                    BankDetails = new Shared.Integration.Models.PaymentDetails.BankDetails() { Bank = customerInFile.BankID, BankAccount = customerInFile.BankAccount, BankBranch = customerInFile.BankBranch/*, PaymentType = Shared.Integration.Models.PaymentTypeEnum.Bank* todo to check*/ },
                    BillingDesktopRefNumber = customerInFile.DealID,
                    ConsumerAddress = new Shared.Integration.Models.Address() { City = customerInFile.CityID, Street = customerInFile.Street, Zip = customerInFile.ZipCode },
                    ConsumerName = string.Format("{0} {1}", customerInFile.LastName, customerInFile.FirstName),
                    ConsumerNationalID = customerInFile.ClientCode,
                    ConsumerPhone = customerInFile.Phone1,
                    ConsumerID = consumerID,
                    ConsumerPhone2 = customerInFile.Phone2,
                    ExternalReference = string.Format("{0}{1}", RapidClient ? "RPS_" : "", customerInFile.RivCode),
                    Origin = string.Format("BillingDesktop for {0}", MerchantName)//,
                });
            }
            else
            {
                var resCreateCustomer = metadataMerchantService.CreateConsumer(new MerchantProfileApi.Models.Billing.ConsumerRequest
                {
                    Active = customerInFile.Active,
                    BankDetails = new Shared.Integration.Models.PaymentDetails.BankDetails() { Bank = customerInFile.BankID, BankAccount = customerInFile.BankAccount, BankBranch = customerInFile.BankBranch/*, PaymentType = Shared.Integration.Models.PaymentTypeEnum.Bank* todo to check*/ },
                    BillingDesktopRefNumber = customerInFile.DealID,
                    ConsumerAddress = new Shared.Integration.Models.Address() { City = customerInFile.CityID, Street = customerInFile.Street, Zip = customerInFile.ZipCode },
                    ConsumerName = string.Format("{0} {1}", customerInFile.LastName, customerInFile.FirstName),
                    ConsumerNationalID = customerInFile.ClientCode,
                    ConsumerPhone = customerInFile.Phone1,
                    ConsumerSecondPhone = customerInFile.Phone2,
                    ExternalReference = string.Format("{0}{1}", RapidClient ? "RPS_" : "", customerInFile.RivCode),
                    Origin = string.Format("BillingDesktop for {0}", MerchantName)//,
                                                                                  // TerminalID = TerminalG from apikey
                });
                consumerID = resCreateCustomer.Result.EntityUID ?? Guid.Empty;
            }
            return consumerID;
        }

        private static Task<OperationResponse> CreateTokenPerCustomer(string CardNumber, TransactionsApiClient metadataTransactionsService, Guid CustomerID, Customer customerInFile)
        {
            if (!String.IsNullOrEmpty(CardNumber))
            {
                return metadataTransactionsService.CreateToken(new TokenRequest()
                {
                    ConsumerID = CustomerID,
                    CardNumber = customerInFile.CardNumber,
                    CardExpiration = ConvertCardDateToMonthYearcs.GetMonthYearFromCardDate(customerInFile.CardDate),
                    CardOwnerName = string.Format("{0} {1}", customerInFile.LastName, customerInFile.FirstName),
                });
            }
            return null;
        }
        private static void CreateBillingDeal(TransactionsApiClient metadataTransactionsService, Customer customerInFile, Guid TokenCreditCard, SummariesResponse<ConsumerSummary> summeriesReponseConsumer, List<Item> items)
        {
            var AddBillingDeal = metadataTransactionsService.CreateBillingDeal(new Transactions.Api.Models.Billing.BillingDealRequest()
            {
                BankDetails = new Shared.Integration.Models.PaymentDetails.BankDetails() { Bank = customerInFile.BankID, BankAccount = customerInFile.BankAccount, BankBranch = customerInFile.BankBranch },
                BillingSchedule = new Transactions.Shared.Models.BillingSchedule()
                {
                    RepeatPeriodType = Models.Helper.EnumConvertor.ConvertToBillingType(customerInFile.BillingTypeID),
                    StartAt = CalculateDate.GetStartPayDate(customerInFile.BillingDay, customerInFile.StartDate??DateTime.Today),
                    EndAt = customerInFile.finishDate,
                    EndAtType = customerInFile.finishDate == null ? Transactions.Shared.Enums.EndAtTypeEnum.Never : Transactions.Shared.Enums.EndAtTypeEnum.SpecifiedDate,
                    StartAtType = Transactions.Shared.Enums.StartAtTypeEnum.SpecifiedDate,
                },
                CreditCardToken = TokenCreditCard,
                //Currency =  todo
                //InvoiceDetails = new Shared.Integration.Models.Invoicing.InvoiceDetails() {}
                PaymentType = Models.Helper.EnumConvertor.ConvertToPaymentType(customerInFile.PayType),
                TransactionAmount = customerInFile.TotalSum,

                DealDetails = new Shared.Integration.Models.DealDetails()
                {
                    ConsumerAddress = summeriesReponseConsumer.Data.GetEnumerator().Current.ConsumerAddress,
                    ConsumerEmail = summeriesReponseConsumer.Data.GetEnumerator().Current.ConsumerEmail,
                    //ConsumerExternalReference = summeriesReponseConsumer.Data.GetEnumerator().Current. TODO TO DO 
                    ConsumerID = summeriesReponseConsumer.Data.GetEnumerator().Current.ConsumerID,
                    ConsumerName = summeriesReponseConsumer.Data.GetEnumerator().Current.ConsumerName,
                    ConsumerNationalID = summeriesReponseConsumer.Data.GetEnumerator().Current.ConsumerNationalID,
                    ConsumerPhone = summeriesReponseConsumer.Data.GetEnumerator().Current.ConsumerPhone,
                    Items = items
                    //DealReference = 
                }
            });
        }
    }

}


