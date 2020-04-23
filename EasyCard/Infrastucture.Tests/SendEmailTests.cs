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
        [Fact(DisplayName = "Send Email To Event Hub")]
        public void SendEmailToEventHub()
        {
            var EmailEventHubConnectionString = "Endpoint=sb://ecnghub.servicebus.windows.net/;SharedAccessKeyName=sender;SharedAccessKey=pEUgdyZt/jCDnQMtuPlU1npBDCC6D9g+g74KGPwRo58=;EntityPath=email";
            var EmailEventHubName = "email";

            EventHubEmailSender emailSender = new EventHubEmailSender(EmailEventHubConnectionString, EmailEventHubName);

            var emailMessage = new Email
            {
                TemplateCode = "TestEmail",
                EmailTo = "vit.muromsky@gmail.com",
                Subject = "Test message",
                Substitutions = new TextSubstitution[] { new TextSubstitution { Substitution = "{test}", Value = "Test Message" } }
            };

            emailSender.SendEmail(emailMessage).Wait();
        }
    }
}
