using System.Text.Encodings.Web;
using System.Threading.Tasks;
using IdentityServer.Messages;
using IdentityServer.Models;
using Shared.Helpers.Email;

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
                Substitutions = new EmailSubstitution[] { new EmailSubstitution { Substitution = "{confirmationLink}", Value = HtmlEncoder.Default.Encode(link) } }
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
                Substitutions = new EmailSubstitution[] { new EmailSubstitution { Substitution = "{resetPasswordLink}", Value = HtmlEncoder.Default.Encode(link) } }
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
                Substitutions = new EmailSubstitution[] { new EmailSubstitution { Substitution = "{code}", Value = HtmlEncoder.Default.Encode(code) } }
            };

            await emailSender.SendEmail(emailMessage);
        }
    }
}
