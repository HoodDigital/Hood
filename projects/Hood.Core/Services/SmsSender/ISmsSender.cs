using System.Threading.Tasks;

namespace Hood.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
