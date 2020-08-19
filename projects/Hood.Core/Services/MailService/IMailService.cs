using Hood.Interfaces;
using Hood.Models;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IMailService
    {
        Task<Response> ProcessAndSend(IEmailSendable model);
    }
}
