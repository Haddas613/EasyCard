using System;
using System.IO;
using System.Threading.Tasks;

namespace DesktopEasyCardConvertorECNG
{
    class Program
    {
        static void Main(string[] args)
        {
            // string AuthToken = Common.Helpers.Settings.GetAuthorizationCode();
            // bool merchantTerminalExist = false;
            // bool billingAllowed = false;
            //  Common.Helpers.Settings.CheckSettings("d09bd9bb-f371-4df0-9c7e-abdc0069b415", "95297b3b-2595-4f72-82f7-abdc006a1c2a", out merchantTerminalExist, out billingAllowed);

            var task = Task.Run(async () => await Common.Helpers.ReadMDBFile.ReadDataFromMDBFile("C:\\Users\\hadas\\Documents\\Convert\\ConvertDesktopOnline\\EasyCardBillingComplexe\\EasyCardBilling.mdb"));
            task.Wait();
            Common.Helpers.Settings.UpdateTerminalSettings("95297b3b-2595-4f72-82f7-abdc006a1c2a","","","",1.32M,1.3M,true);
            
            decimal maaxPercent = 17;
            bool IsConstRate = false;
            double constDollar = Common.Helpers.Currency.GetApiRate( Common.Models.Enums.CurrencyEnum.DOLLAR);
            double constEuro = Common.Helpers.Currency.GetApiRate(Common.Models.Enums.CurrencyEnum.EURO);
            int clientID = 3097;
            bool RapidClient = false;
            string MDBFilePath = "";
            bool IsExistMDBFileInPath = false;
            Console.WriteLine("Enter Full Path of MDB file; for example: C:\\\\Users\\\\hadas\\\\Desktop\\\\ConvertDesktopOnline\\\\EasyCardBilling_Before16042020Fix.mdb");
            MDBFilePath = Console.ReadLine();
            IsExistMDBFileInPath = File.Exists(MDBFilePath);
            while (String.IsNullOrEmpty(MDBFilePath) || !IsExistMDBFileInPath)
            {
                Console.WriteLine("Failed to find MDB file in path,please insert again:");
                MDBFilePath = Console.ReadLine();
                IsExistMDBFileInPath = File.Exists(MDBFilePath);
            }

            Console.WriteLine("Enter EasyCard NG  MerchantID:");
            string MerchantID = Console.ReadLine();


            Console.WriteLine("Enter EasyCard NG  TerminalID:");
            string TerminalID = Console.ReadLine();


            Console.WriteLine("Enter true if the client is Rapid Client or false if not:");
            string RapidClientStr = Console.ReadLine();
            while (!Boolean.TryParse(RapidClientStr, out RapidClient))
            {
                Console.WriteLine("Error typing, Please type true or false");
                RapidClientStr = Console.ReadLine();
            }

            Console.WriteLine("Enter true to work with const foreign currency, false if online convertor");
            string IsConstRateStr = Console.ReadLine();
            while (!Boolean.TryParse(IsConstRateStr, out IsConstRate))
            {
                Console.WriteLine("Error typing, Please type true or false");
                IsConstRateStr = Console.ReadLine();
            }

            if (IsConstRate)
            {
                Console.WriteLine("Enter rate value for dollar");
                string constDollarStr = Console.ReadLine();
                while (!Double.TryParse(constDollarStr, out constDollar))
                {
                    Console.WriteLine("Error typing rate value for dollar (should be only numbers)");
                    constDollarStr = Console.ReadLine();
                }

                Console.WriteLine("Enter rate value for euro");
                string constEuroStr = Console.ReadLine();
                while (!Double.TryParse(constEuroStr, out constEuro))
                {
                    Console.WriteLine("Error typing rate value for euro (should be only numbers)");
                    constEuroStr = Console.ReadLine();
                }
            }

            bool merchantTerminalExist = false;
            bool billingAllowed = false;
            Common.Helpers.Settings.CheckSettings("d09bd9bb-f371-4df0-9c7e-abdc0069b415", "95297b3b-2595-4f72-82f7-abdc006a1c2a", out merchantTerminalExist, out billingAllowed);

            // bool existTerminalNMerchant = Common.Helpers.Settings.CheckMerchantNTerminalExist(MerchantID, TerminalID);//CheckMerchantNTerminalExist("d09bd9bb-f371-4df0-9c7e-abdc0069b415", "95297b3b-2595-4f72-82f7-abdc006a1c2a");
            while (!merchantTerminalExist)
            {
                Console.WriteLine("Terminal or/and Merchant is not exist");
                Console.ReadLine();
            }

           // bool billingAllowed = Common.Helpers.Settings.CheckBillingAllowed(TerminalID);
            while (!billingAllowed)
            {
                Console.WriteLine("Billing is not allowed for this TerminalID");
                Console.ReadLine();
            }

            //get termina details

            //update terminal details

            //create 


            Console.ReadLine();
        }
    }
}
