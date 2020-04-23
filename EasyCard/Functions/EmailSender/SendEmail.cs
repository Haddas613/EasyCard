using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using Shared.Helpers.Email;
using Shared.Helpers.Templating;

namespace EmailSender
{
    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static async Task Run([EventHubTrigger("email", Connection = "emailqueue")] string messageBody, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"SendEmail function started at {DateTime.UtcNow}");

            var exceptions = new List<Exception>();

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var table = await GetEmailTemplatesStorage(config);

            //var sendGridClient = new SendGridClient(config["SendgridApiKey"]);

            log.LogInformation(messageBody);
            var emailData = JsonConvert.DeserializeObject<Email>(messageBody);

            log.LogInformation($"Email sending for {emailData.EmailTo} began. Message: {messageBody}");

            var template = (await GetEmailTemplate(table, emailData.TemplateCode))
            ?? throw new Exception($"Template {emailData.TemplateCode} does not exist");

            var emailSubject = TemplateProcessor.Substitute(template.SubjectTemplate, emailData.Substitutions);
            var emailBody = TemplateProcessor.Substitute(template.BodyTemplate, emailData.Substitutions);

            var message = new SendGridMessage();
            message.Subject = emailSubject;
            message.HtmlContent = emailBody;
            message.AddTo(emailData.EmailTo);
            message.SetFrom(config["EmailFrom"]);

            //TODO: uncomment when credentials are ready
            //var result = await sendGridClient.SendEmailAsync(message);
            //log.LogInformation($"Email sending for {emailData.EmailTo} finished. Status code: {result.StatusCode}");
            log.LogInformation($"Email sending for {emailData.EmailTo} finished.");

            log.LogInformation($"SendEmail function completed at {DateTime.UtcNow} without errors");
        }

        private static async Task<CloudTable> GetEmailTemplatesStorage(IConfigurationRoot cfg)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cfg["AzureWebJobsStorage"]);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(cfg["EmailTemplatesStorageTable"]);

            return await Task.FromResult(table);
        }

        private static async Task<EmailTemplateEntity> GetEmailTemplate(CloudTable table, string templateCode)
        {
            TableOperation getOperation = TableOperation.Retrieve<EmailTemplateEntity>(EmailTemplateEntity.DefaultPartitionKey, templateCode);

            var result = await table.ExecuteAsync(getOperation);

            if (result.Result != null)
            {
                return (EmailTemplateEntity)result.Result;
            }
            else
            {
                return null;
            }
        }
    }
}
