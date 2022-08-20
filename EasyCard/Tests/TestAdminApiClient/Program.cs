using DesktopEasyCardConvertorECNG;
using Shared.Api.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Client;
using Transactions.Api.Models.Transactions;

namespace TestAdminApiClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceFactory = new ServiceFactory("yuwsCT8cbJVgw2W6", DesktopEasyCardConvertorECNG.Environment.DEV);


            var transactionsService = serviceFactory.GetTransactionsApiClient();


            try
            {
                string terminalid = "152fa179-3050-4b62-baf7-aee600d1415e";

                var createRequest = new Transactions.Api.Models.Transactions.CreateTransactionRequest
                {

                    TransactionAmount = 1,
                    TerminalID = Guid.Parse(terminalid),
                    CreditCardSecureDetails = new Shared.Integration.Models.CreditCardSecureDetails { CardNumber = "4557430402053720", CardExpiration = new Shared.Helpers.CardExpiration { Month = 6, Year = 26 }, CardOwnerName = "Avinoam", CardOwnerNationalID = "122222227", Cvv = "393" }
                };

                bool resTask = RunTasks(createRequest, transactionsService).Result;
                if(resTask)
                    Console.WriteLine(" true");
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create transaction : {ex.Message}");
            }
        }

        static async Task<bool> RunTasks(CreateTransactionRequest createRequest, TransactionsApiClient transactionsService)
        {
            Task<OperationResponse> t1 = CreateTransactionTask(transactionsService, createRequest);
            Task<OperationResponse> t2 = CreateTransactionTask(transactionsService, createRequest);

            var res = await Task.WhenAll(t1, t2);
            return res.Any(d => d.Status == Shared.Api.Models.Enums.StatusEnum.Success) && res.Any(d => d.Errors?.Any(x => x.Code == "DoubleTansactions") == true && d.Status == Shared.Api.Models.Enums.StatusEnum.Error);
        }

       static async Task<OperationResponse> CreateTransactionTask(TransactionsApiClient transactionsService, CreateTransactionRequest createRequest)
        {
            return await transactionsService.CreateTransaction(createRequest);
        }
    }
}
