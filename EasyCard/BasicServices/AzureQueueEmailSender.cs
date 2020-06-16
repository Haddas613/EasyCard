using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using Shared.Helpers.Email;
using System;
using System.Text;
using System.Threading.Tasks;

namespace BasicServices
{
    /// <summary>
    /// EmailSender based on Azure EventHub
    /// </summary>
    /// <remarks>
    /// Note: use as singleton
    /// </remarks>
    public class AzureQueueEmailSender : IEmailSender, IDisposable
    {
        private readonly EventHubClient eventHubClient;

        public AzureQueueEmailSender(string connectionString, string eventHubName)
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(connectionString)
            {
                EntityPath = eventHubName
            };

            this.eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }

        public void Dispose()
        {
            if (eventHubClient != null)
            {
                eventHubClient.Close();
            }
        }

        public async Task SendEmail(Email email)
        {
            var message = JsonConvert.SerializeObject(email);

            await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)), DateTime.UtcNow.ToString("yy-MM-dd"));
        }
    }
}
