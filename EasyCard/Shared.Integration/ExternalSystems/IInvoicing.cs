using Newtonsoft.Json.Linq;
using Shared.Integration.Models.Invoicing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Integration.ExternalSystems
{
    public interface IInvoicing
    {
        Task<InvoicingCreateDocumentResponse> CreateDocument(InvoicingCreateDocumentRequest documentCreationRequest);

        Task<IEnumerable<string>> GetDownloadUrls(JObject externalSystemData, object invoiceingSettings);

        Task<IEnumerable<IntegrationMessage>> GetStorageLogs(string entityID);

        Task<CreateConsumerResponse> CreateConsumerOrGetExisting(CreateConsumerRequest consumerRequest);

        bool CanCreateConsumer();
    }
}
