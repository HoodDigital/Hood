using System.Threading.Tasks;

namespace Hood.Services
{
    public interface ISmsSender
    {
        // ReSharper disable twice UnusedParameter.Global — SMS contract; the bundled sender is a stub, real implementations need both.
        Task SendSmsAsync(string number, string message);
    }
}
