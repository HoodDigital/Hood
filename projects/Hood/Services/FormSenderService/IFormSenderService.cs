using Hood.Models;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IMailSenderService<TEmailModel>
        where TEmailModel : IEmailSendable
    {
        Task<Response> ProcessAndSend(TEmailModel model);
    }
}
