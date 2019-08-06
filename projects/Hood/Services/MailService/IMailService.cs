using Hood.Models;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IMailService
    {
        Task<Response> ProcessAndSend(IEmailSendable model);
    }
}
