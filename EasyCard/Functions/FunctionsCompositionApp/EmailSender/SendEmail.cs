using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using Shared.Helpers.Email;
using Shared.Helpers.Templating;

namespace EmailSender
{
    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static async Task Run([QueueTrigger("email")] string messageBody, ILogger log, ExecutionContext context, [SendGrid(ApiKey = "SendgridApiKey")] IAsyncCollector<SendGridMessage> messageCollector)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(config["AzureWebJobsStorage"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable emailTable = tableClient.GetTableReference(config["EmailTableName"] ?? "email");
            CloudTable emailTemplatesStorageTable = tableClient.GetTableReference(config["EmailTemplatesStorageTable"] ?? "emailtemplates");

            log.LogInformation(messageBody);

            var emailMessage = JsonConvert.DeserializeObject<EmailQueueMessage>(messageBody);

            var emailData = await GetEmailMessage(emailTable, emailMessage)
            ?? throw new Exception($"Message {messageBody} does not exist");

            var template = (await GetEmailTemplate(emailTemplatesStorageTable, emailData.TemplateCode))
            ?? throw new Exception($"Template {emailData.TemplateCode} does not exist");

            var substitutions = JsonConvert.DeserializeObject<IEnumerable<TextSubstitution>>(emailData.Substitutions);

            var emailSubject = TemplateProcessor.Substitute(template.SubjectTemplate, substitutions);
            var emailBody = TemplateProcessor.Substitute(template.BodyTemplate, substitutions);


            var message = new SendGridMessage();
            message.AddTo(emailData.EmailTo);
            message.AddContent("text/html", emailBody);
            message.SetFrom(new EmailAddress(config["EmailFrom"]));
            message.SetSubject(emailSubject);

            await messageCollector.AddAsync(message);

            // TODO: update sent date

            log.LogInformation($"Email sending for {emailData.EmailTo} finished. Status code:");
        }

        private static async Task<EmailEntity> GetEmailMessage(CloudTable emailTable, EmailQueueMessage message)
        {
            TableOperation getOperation = TableOperation.Retrieve<EmailEntity>(message.MessageDate.ToString("yy-MM-dd"), message.MessageID.ToString());

            var result = await emailTable.ExecuteAsync(getOperation);

            if (result.Result != null)
            {
                return (EmailEntity)result.Result;
            }
            else
            {
                return null;
            }
        }

        private static async Task<EmailTemplateEntity> GetEmailTemplate(CloudTable emailTemplatesStorageTable, string templateCode)
        {
            TableOperation getOperation = TableOperation.Retrieve<EmailTemplateEntity>(EmailTemplateEntity.DefaultPartitionKey, templateCode);

            var result = await emailTemplatesStorageTable.ExecuteAsync(getOperation);

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
