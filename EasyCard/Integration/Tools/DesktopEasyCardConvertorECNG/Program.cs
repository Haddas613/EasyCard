using MerchantProfileApi.Models.Billing;
using Shared.Api.Models;
using System;

namespace DesktopEasyCardConvertorECNG
{
    class Program
    {
        static void Main(string[] args)
        {
            bool RapidClient = false;
            string MerchantName = "Test";

            Console.WriteLine("Enter EasyCard NG  MerchantID:");
            string MerchantID = Console.ReadLine();


            Console.WriteLine("Enter EasyCard NG  TerminalID:");
            string TerminalID = Console.ReadLine();
            Guid TerminalG;
            Guid.TryParse(TerminalID, out TerminalG);
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
            #region Create__Update_Customer
            foreach (var customerInFile in dataFromFile.Customers)
            {
                ConsumersFilter cf = new ConsumersFilter();
                cf.BillingDesktopRefNumber = customerInFile.DealID;
                var findcustomer = metadataMerchantService.GetConsumers(cf);
                if (findcustomer.Result.NumberOfRecords > 0)
                {
                    var resCreateCustomer = metadataMerchantService.UpdateConsumer(new MerchantProfileApi.Models.Billing.UpdateConsumerRequest
                    {
                        Active = customerInFile.Active,
                        BankDetails = new Shared.Integration.Models.PaymentDetails.BankDetails() { Bank = customerInFile.BankID, BankAccount = customerInFile.BankAccount, BankBranch = customerInFile.BankBranch/*, PaymentType = Shared.Integration.Models.PaymentTypeEnum.Bank* todo to check*/ },
                        BillingDesktopRefNumber = customerInFile.DealID,
                        ConsumerAddress = new Shared.Integration.Models.Address() { City = customerInFile.CityID, Street = customerInFile.Street, Zip = customerInFile.ZipCode },
                        ConsumerName = string.Format("{0} {1}", customerInFile.LastName, customerInFile.FirstName),
                        ConsumerNationalID = customerInFile.ClientCode,
                        ConsumerPhone = customerInFile.Phone1,
                        ConsumerID = findcustomer.Result.Data.GetEnumerator().Current.ConsumerID,
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
                        ConsumerPhone2 = customerInFile.Phone2,
                        ConsumerSecondPhone = customerInFile.Phone2,
                        ExternalReference = string.Format("{0}{1}", RapidClient ? "RPS_" : "", customerInFile.RivCode),
                        Origin = string.Format("BillingDesktop for {0}", MerchantName)//,
                                                                                      // TerminalID = TerminalG from apikey
                    });
                }
            }
            #endregion  Create_Customer

            #region Add_Items
            var AddUtemsRes =  metadataTerminalService.
            #endregion Add_Items
            // var res = metadataService.CreateConsumer(new MerchantProfileApi.Models.Billing.ConsumerRequest
            //{

            // ConsumerName = "Test from conversion tool",
            //     ConsumerPhone = "798695987987",
            //     ConsumerEmail = "testtool@gmail.com"

            //}).Result;

            //Console.WriteLine(res.Status);

            // var res = ReadMDBFile.ReadDataFromMDBFile(null).Result;

            Console.WriteLine("success"/*res.Customers?.Count*/);
        }
    }
}
