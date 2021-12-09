using DesktopEasyCardConvertorECNG;
using System;

namespace TestAdminApiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceFactory = new ServiceFactory("FLMpBqu8gD3UXw8AAWT+hZMHMCzz8WWR8nEOAhcR8tQwn/4/X4OSO1oQ+td7A3IuzUWhsdJfbQhH4QLfGsnA4w==", DesktopEasyCardConvertorECNG.Environment.LIVE);


            //var metadataMerchantService = serviceFactory.GetMerchantMetadataApiClient();
            var transactionsService = serviceFactory.GetTransactionsApiClient();

            var trans = transactionsService.GetTransactions(new Transactions.Api.Models.Transactions.TransactionsFilter
            {
                TerminalID = Guid.Parse("a773426f-acf4-41c9-8738-adef00cd9b21"),
                Statuses = new System.Collections.Generic.List<Transactions.Shared.Enums.TransactionStatusEnum> { Transactions.Shared.Enums.TransactionStatusEnum.Completed },
                Take = 1000
            }).Result;

            foreach (var tran in trans.Data)
            {
                if (tran.InvoiceID == null)
                {
                    Console.WriteLine(tran.PaymentTransactionID);

                    var res = transactionsService.CreateInvoiceForTransaction(tran.PaymentTransactionID).Result;

                    Console.WriteLine(res.EntityUID);
                }
                else
                {
                    Console.WriteLine(tran.InvoiceID);
                }
            }

            //var res = transactionsService.CreateInvoiceForTransaction(Guid.Parse("41F6110E-0ADE-44E0-95B9-ADF700A4D041")).Result;

            //Console.WriteLine(res);

            //var res2 = transactionsService.GenerateInvoice(Guid.Parse("3f36f55e-bb2a-4a37-90f2-adf700d743f4")).Result;

            //Console.WriteLine(res2);
        }
    }
}
