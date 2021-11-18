using System;

namespace DesktopEasyCardConvertorECNG
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceFactory = new ServiceFactory(args[0], Environment.QA);

            var metadataService = serviceFactory.GetMerchantMetadataApiClient();

            var res = metadataService.CreateConsumer(new MerchantProfileApi.Models.Billing.ConsumerRequest
            {
                 ConsumerName = "Test from conversion tool",
                 ConsumerPhone = "798695987987",
                 ConsumerEmail = "testtool@gmail.com"
                   
            }).Result;

            Console.WriteLine(res.Status);
        }
    }
}
