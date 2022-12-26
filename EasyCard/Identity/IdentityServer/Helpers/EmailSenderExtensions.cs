using System.Text.Encodings.Web;
using System.Threading.Tasks;
using IdentityServer.Messages;
using IdentityServer.Models;
using Shared.Helpers.Email;
using Shared.Helpers.Templating;

namespace IdentityServer.Helpers
{
    public static class EmailSenderExtensions
    {
        public static async Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            var emailMessage = new Email
            {
                TemplateCode = EmailTemplateCodes.ConfirmationEmail,
                EmailTo = email,
                Subject = IdentityMessages.ConfirmAccount,
                Substitutions = new TextSubstitution[] { new TextSubstitution { Substitution = "confirmationLink", Value = HtmlEncoder.Default.Encode(link) } }
            };

            await emailSender.SendEmail(emailMessage);
        }

        public static async Task SendEmailLinkedToMerchantAsync(this IEmailSender emailSender, string email, string link, string merchantName)
        {
            var emailMessage = new Email
            {
                TemplateCode = EmailTemplateCodes.UserLinkedToMerchant,
                EmailTo = email,
                Subject = IdentityMessages.UserLinkedToMerchant,
                Substitutions = new TextSubstitution[]
                {
                    new TextSubstitution { Substitution = "merchantProfileLink", Value = HtmlEncoder.Default.Encode(link) },
                    new TextSubstitution { Substitution = "merchantName", Value = merchantName }
                }
            };

            await emailSender.SendEmail(emailMessage);
        }

        public static async Task SendEmailResetPasswordAsync(this IEmailSender emailSender, string email, string link)
        {
            var emailMessage = new Email
            {
                TemplateCode = EmailTemplateCodes.ResetPasswordEmail,
                EmailTo = email,
                Subject = IdentityMessages.ResetPassword,
                Substitutions = new TextSubstitution[] { new TextSubstitution { Substitution = "resetPasswordLink", Value = HtmlEncoder.Default.Encode(link) } }
            };

            await emailSender.SendEmail(emailMessage);
        }

        public static async Task Send2faEmailAsync(this IEmailSender emailSender, string email, string code)
        {
            var emailMessage = new Email
            {
                TemplateCode = EmailTemplateCodes.TwoFactorAuth,
                EmailTo = email,
                Subject = IdentityMessages.TwoFactorAuth,
                Substitutions = new TextSubstitution[] { new TextSubstitution { Substitution = "code", Value = HtmlEncoder.Default.Encode(code) } }
            };

            await emailSender.SendEmail(emailMessage);
        }
    }
}
