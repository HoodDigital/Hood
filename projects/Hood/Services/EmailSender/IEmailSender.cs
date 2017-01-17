using Hood.Infrastructure;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IEmailSender
    {
        Task<OperationResult> SendEmail(MailObject message, string template = Models.MailSettings.PlainTemplate);
    }
}
