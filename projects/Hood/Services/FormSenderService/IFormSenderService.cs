using Hood.Models;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IMailSenderService<TEmailModel>
        where TEmailModel : IEmailSendable
    {
        Task<Response> ProcessAndSend(TEmailModel model,
                                        string NotifyRole = "ContactFormNotifications",
                                        EmailAddress to = null,
                                        EmailAddress from = null,
                                        string NotificationTitle = null,
                                        string NotificationMessage = null,
                                        string AdminNotificationTitle = null,
                                        string AdminNotificationMessage = null);
    }
}
