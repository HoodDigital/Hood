using System.Threading.Tasks;
using System;
using Hood.Models;
using Hood.Extensions;

namespace Hood.Services
{
    public class MailService : IMailService
    {
        private readonly IEmailSender _email;
        private readonly ILogService _logService;

        public MailService(IEmailSender email, ILogService logService)
        {
            _email = email;
            _logService = logService;
        }
#warning TODO: Ensure this is only used in try/catch - and remove the exception handling from the method, so error bubbles up.
        public async Task<Response> ProcessAndSend(IEmailSendable model)
        {
            try
            {
                MailObject message = new MailObject();
                message = model.WriteToMailObject(message);

                if (model.SendToRecipient)
                {
                    message.To = model.To;
                    await _email.SendEmailAsync(message, model.From);
                }

                message = new MailObject();
                message = model.WriteNotificationToMailObject(message);

                if (model.NotifyEmails != null)
                {
                    foreach (var recipient in model.NotifyEmails)
                    {
                        message.To = recipient;
                        await _email.SendEmailAsync(message, model.From);
                    }
                }

                if (model.NotifyRole.IsSet())
                    await _email.NotifyRoleAsync(message, model.NotifyRole, model.From);

                return new Response(true, $"The message has been sent.");
            }
            catch (Exception sendEx)
            {
                await _logService.AddExceptionAsync<MailService>("There was a problem sending the message: " + sendEx.Message, sendEx);
                throw new Exception("There was a problem sending the message.", sendEx);
            }
        }


    }
}