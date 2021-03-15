using BasicServices;
using Shared.Helpers.Email;
using Shared.Helpers.Templating;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using Xunit;

namespace Infrastucture.Tests
{
    public class SendEmailTests
    {
        [Fact(DisplayName = "Send Email To Azure Queue")]
        public void SendEmailToQueue()
        {
            var DefaultStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=ecng;AccountKey=4NjfZ4WcFlvNBzKHgbyGDdl+iYBiUv1SPU2hVneIqDyX0TsHUXtG707cfrGxnOCHD85L8mLRamck9w014/m1Vg==;EndpointSuffix=core.windows.net";
            var EmailQueueName = "email";
            var EmailTableName = "email";

            var queue = new AzureQueue(DefaultStorageConnectionString, EmailQueueName);
            var emailSender = new AzureQueueEmailSender(queue, DefaultStorageConnectionString, EmailTableName);
            

            var emailMessage = new Email
            {
                TemplateCode = "TestEmail",
                EmailTo = "volkovv@mydigicode.com",
                Subject = "Test message",
                Substitutions = new TextSubstitution[] { new TextSubstitution { Substitution = "test", Value = "Test Message" } }
            };

            emailSender.SendEmail(emailMessage).Wait();
        }
    }
}
