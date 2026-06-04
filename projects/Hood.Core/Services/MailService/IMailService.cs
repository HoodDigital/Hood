using System.Threading.Tasks;
using Hood.Interfaces;
using Hood.Models;

namespace Hood.Services
{
    public interface IMailService
    {
        Task<Response> ProcessAndSend(IEmailSendable model);
    }
}
