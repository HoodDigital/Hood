using Hood.Infrastructure;
using Hood.Models;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IFormSenderService
    {
        Task<Response> ProcessContactFormModel(IContactFormModel model);
    }
}
