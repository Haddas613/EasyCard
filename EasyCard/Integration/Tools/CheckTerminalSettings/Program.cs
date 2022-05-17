using CsvHelper;
using CsvHelper.Configuration;
using DesktopEasyCardConvertorECNG;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RapidOne.DTOs.PaymentIntegration;
using RapidOne.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Environment = DesktopEasyCardConvertorECNG.Environment;

namespace CheckTerminalSettings
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .Build();

            var appConfig = Configuration.GetSection("AppConfig").Get<ApplicationSettings>();

            var serviceFactory = new ServiceFactory(Environment.LIVE);

            var key = Convert.FromBase64String(appConfig.EncrKey);
            var iv = Convert.FromBase64String(appConfig.EncrIv);

            var encr = new EncryptionHelper(key, iv);

            var paymentSystemsConfigs = GetPaymentSystemConfigs(serviceFactory, encr, appConfig.PaymentSystemsConfigsTsv);

            var res = ReadAllBillingsFromFile(appConfig.BillingsTsv);

            CheckAllBillings(res, paymentSystemsConfigs, serviceFactory, appConfig.BillingsTsv);

        }

        private static void CheckAllBillings(IEnumerable<R1Subsription> subsriptions, List<OutRecord> paymentSystems, ServiceFactory serviceFactory, string fileName)
        {
            int n = 1;

            List<Report> res = new List<Report>();

            var today = DateTime.Today;

            foreach (var issuer in subsriptions.Select(d => d.DefaultIssuer).Distinct().ToList())
            {
                var paymentSystemConfigs = paymentSystems.Where(d => d.IssuerId == issuer && d.IsBilling == 1 && d.Enabled == 1);

                if (paymentSystemConfigs.Count() != 1)
                {
                    Console.WriteLine($"Failed to retreive paymentSystemConfig for {issuer}");
                    continue;
                }

                var paymentSystemConfig = paymentSystemConfigs.Single();
                Console.WriteLine(paymentSystemConfig.PrivateKey);


                var transactionsApi = serviceFactory.GetTransactionsApiClient(paymentSystemConfig.PrivateKey);

                foreach(var subsription in subsriptions.Where(d => d.DefaultIssuer == issuer))
                {
                    DateTime? endDate = subsription.EndDate == "NULL" ? null : (DateTime?)DateTime.Parse(subsription.EndDate);
                    DateTime? startDate = subsription.StartDate == "NULL" ? null : (DateTime?)DateTime.Parse(subsription.StartDate);

                    var subsriptionActive = endDate == null || endDate > today;

                    var rep = new Report
                    {
                        BillingDealID = subsription.BillingDealID,
                        BranchId = subsription.BranchId,
                        CustomerId = subsription.CustomerId,
                        DefaultIssuer = subsription.DefaultIssuer,
                        EndDate = subsription.EndDate,
                        Id = subsription.Id,
                        Price = subsription.Price,
                        StartDate = subsription.StartDate,
                        EcngStartDate = null,
                        TerminalID = paymentSystemConfig.TerminalID,
                        Active = subsriptionActive ? 1 : 0,
                        EcngAtive = null
                    };

                    try
                    {
                        var billingEcng = transactionsApi.GetBillingDeal(Guid.Parse(subsription.BillingDealID)).Result;
                        Console.WriteLine($"{n}\t{subsription.BillingDealID}");

                        rep.EcngStartDate = billingEcng.BillingSchedule.StartAt?.ToString("yyyy-MM-dd");
                        rep.EcngAtive = billingEcng.Active ? 1 : 0;

                        if (subsriptionActive != billingEcng.Active)
                        {
                            rep.Error = "NOT_ACTIVE";
                            Console.WriteLine($"{rep.Error} {subsriptionActive} {billingEcng.Active}");
                        }

                        if (subsription.CustomerId != billingEcng.DealDetails.ConsumerExternalReference)
                        {
                            rep.Error = "CUTOMER";
                            Console.WriteLine($"{rep.Error} {subsription.CustomerId} {billingEcng.DealDetails.ConsumerExternalReference}");
                        }

                        var price = Convert.ToDecimal(subsription.Price);
                        if (price != billingEcng.TransactionAmount)
                        {
                            rep.Error = "AMOUNT";
                            Console.WriteLine($"{rep.Error} {price} {billingEcng.TransactionAmount}");
                        }

                        Console.WriteLine($"{startDate} {billingEcng.BillingSchedule.StartAt}");
                        if (startDate > billingEcng.BillingSchedule.StartAt)
                        {
                            rep.Error = "START_DATE";
                            Console.WriteLine($"{rep.Error} {startDate} {billingEcng.BillingSchedule.StartAt}");
                        }

                        if (startDate.GetValueOrDefault().Day != billingEcng.BillingSchedule.StartAt.GetValueOrDefault().Day)
                        {
                            rep.Error = "START_DATE_DAY";
                            Console.WriteLine($"{rep.Error} {startDate.GetValueOrDefault().Day} {billingEcng.BillingSchedule.StartAt.GetValueOrDefault().Day}");
                        }

                        //if (rep.Error != null)
                        {
                            res.Add(rep);
                        }

                    }
                    catch(Exception ex)
                    {
                        rep.Error = "NOT_FOUND";

                        res.Add(rep);

                        var nfex = ex.InnerException as Shared.Helpers.WebApiClientErrorException;
                        if (nfex != null)
                        {
                            Console.WriteLine($"Cannot retreive billing {subsription.BillingDealID}: {nfex.StatusCode}");
                        }
                        else
                        {
                            Console.WriteLine($"Cannot retreive billing {subsription.BillingDealID}: {ex.Message}");
                        }
                    }

                    n++;
                }
            }


            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "\t"
            };

            var outFile = fileName.Replace(".tsv", ".out.tsv");
            using (var stream = File.Open(outFile, FileMode.Create))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, csvConfig))
            {
                csv.WriteRecords(res);
            }
        }

        private static IEnumerable<R1Subsription> ReadAllBillingsFromFile(string fileName)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "\t"
            };

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                var records = csv.GetRecords<R1Subsription>().ToList();

                Console.WriteLine($"Found {records.Count()} billings");

                //foreach (var record in records)
                //{
                //    Console.WriteLine(record.BillingDealID);
                //}

                return records;
            }

            
        }

        private static List<OutRecord> GetPaymentSystemConfigs(ServiceFactory serviceFactory, EncryptionHelper encr, string fileName)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "\t"
            };

            List<OutRecord> outRecords = new List<OutRecord>();

            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                var records = csv.GetRecords<R1PaymentSYstemConfig>().ToList();

                foreach (var record in records)
                {
                    var cfgStr = encr.Decrypt(record.PaymentSystemConfig);

                    var paymentSystemConfig = JsonConvert.DeserializeObject<ECNGConfig>((string)cfgStr);

                    var metadataMerchantService = serviceFactory.GetMerchantMetadataApiClient(paymentSystemConfig.SecretKey);

                    var terminal = metadataMerchantService.GetTerminals().Result.Data.FirstOrDefault();

                    Console.WriteLine(terminal.TerminalID);

                    outRecords.Add(new OutRecord
                    {
                        Id = record.Id,
                        TerminalID = terminal.TerminalID.ToString(),
                        IssuerId = record.IssuerId,
                        Enabled = ConvertToInt32(record.Enabled),
                        IsCustomerPortalTerminal = ConvertToInt32(record.IsCustomerPortalTerminal),
                        BranchIds = record.BranchIds,
                        IsBilling = ConvertToInt32(record.IsBilling),
                        MappingBranchId = ConvertToInt32(record.MappingBranchId),
                        PrivateKey = paymentSystemConfig.SecretKey,
                    });
                }
            }

            return outRecords;

            //var outFile = fileName.Replace(".tsv", ".out.tsv");
            //using (var stream = File.Open(outFile, FileMode.Create))
            //using (var writer = new StreamWriter(stream))
            //using (var csv = new CsvWriter(writer, csvConfig))
            //{
            //    csv.WriteRecords(outRecords);
            //}
        }

        private static int ConvertToInt32(string dVal)
        {
            var strVal = (string)dVal;

            if (strVal == "NULL")
            {
                return 0;
            }

            return Convert.ToInt32(strVal);

        }
    }

    public class Report
    {
        public int Id { get; set; }

        public string BillingDealID { get; set; }

        public string TerminalID { get; set; }

        public string DefaultIssuer { get; set; }

        public string BranchId { get; set; }

        public string CustomerId { get; set; }

        public string Price { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string Error { get; set; }

        public string EcngStartDate { get; set; }

        public int Active { get; internal set; }

        public int? EcngAtive { get; internal set; }
    }

    public class OutRecord
    {
        public int Id { get; set; }

        public string TerminalID { get; set; }

        public string IssuerId { get; set; }

        public int Enabled { get; set; }

        public int IsCustomerPortalTerminal { get; set; }

        public string BranchIds { get; set; }

        public int IsBilling { get; set; }

        public int MappingBranchId { get; set; }

        public string PrivateKey { get; set; }
    }

    public class R1PaymentSYstemConfig
    {
        public int Id { get; set; }

        public string PaymentSystemConfig { get; set; }

        public string IssuerId { get; set; }

        public string Enabled { get; set; }

        public string IsCustomerPortalTerminal { get; set; }

        public string BranchIds { get; set; }

        public string IsBilling { get; set; }

        public string MappingBranchId { get; set; }
    }

    public class R1Subsription
    {
        public int Id { get; set; }

        public string BillingDealID { get; set; }

        public string DefaultIssuer { get; set; }

        public string BranchId { get; set; }

        public string CustomerId { get; set; }

        public string Price { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }
    }
}
