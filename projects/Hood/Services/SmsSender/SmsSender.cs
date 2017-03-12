using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Hood.Services
{
    public class SmsSender : ISmsSender
    {
        public SmsSender(IOptions<SmsSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public Task SendSmsAsync(string number, string message)
        {
            return null;
        }

        public SmsSenderOptions Options { get; }  // set only via Secret Manager

    }
}