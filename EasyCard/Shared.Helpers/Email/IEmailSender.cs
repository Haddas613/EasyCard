using System.Threading.Tasks;

namespace Shared.Helpers.Email
{
    public interface IEmailSender
    {
        Task SendEmail(Email email);
    }
}
