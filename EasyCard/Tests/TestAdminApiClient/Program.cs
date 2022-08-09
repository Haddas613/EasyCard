using DesktopEasyCardConvertorECNG;
using Shared.Api.Models;
using System;

namespace TestAdminApiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceFactory = new ServiceFactory("yuwsCT8cbJVgw2W6", DesktopEasyCardConvertorECNG.Environment.DEV);


            //var metadataMerchantService = serviceFactory.GetMerchantMetadataApiClient();
            var transactionsService = serviceFactory.GetTransactionsApiClient();

            var trans = transactionsService.GetTransactions(new Transactions.Api.Models.Transactions.TransactionsFilter
            {
                 DocumentOrigin = Transactions.Shared.Enums.DocumentOriginEnum.Bit, QuickStatusFilter = Transactions.Shared.Enums.QuickStatusFilterTypeEnum.Pending
            }).Result;

            foreach (var transaction in trans.Data)
            {
                try
                {
                    var res = transactionsService.BitTransactionPostProcessing(transaction.PaymentTransactionID).Result;

                    if (res.Status != Shared.Api.Models.Enums.StatusEnum.Success)
                    {
                       Console.WriteLine($"Failed to process Bit transaction {transaction.PaymentTransactionID}: {res.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"Successfully processed Bit transaction {transaction.PaymentTransactionID}: {res.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process Bit transaction {transaction.PaymentTransactionID}: {ex.Message}");
                }
            }


            Guid paymentTransactionRequestID = Guid.NewGuid();
            try
            {
                string terminalid = "152fa179-3050-4b62-baf7-aee600d1415e";


                OperationResponse res = transactionsService.CreateTransaction(new Transactions.Api.Models.Transactions.CreateTransactionRequest { PaymentRequestID = paymentTransactionRequestID, TransactionAmount=1, TerminalID = Guid.Parse(terminalid), CreditCardSecureDetails = new Shared.Integration.Models.CreditCardSecureDetails {  CardNumber= "4557430402053720", CardExpiration = new Shared.Helpers.CardExpiration {  Month=6, Year=26}, CardOwnerName="Avinoam", CardOwnerNationalID="122222227", Cvv="393" }  }).Result;
                transactionsService.CreateTransaction(new Transactions.Api.Models.Transactions.CreateTransactionRequest { PaymentRequestID = paymentTransactionRequestID });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create transaction : {ex.Message}");
            }
            //var res = transactionsService.CreateInvoiceForTransaction(Guid.Parse("41F6110E-0ADE-44E0-95B9-ADF700A4D041")).Result;

            //Console.WriteLine(res);

            //var res2 = transactionsService.GenerateInvoice(Guid.Parse("3f36f55e-bb2a-4a37-90f2-adf700d743f4")).Result;

            //Console.WriteLine(res2);
        }
    }
}
