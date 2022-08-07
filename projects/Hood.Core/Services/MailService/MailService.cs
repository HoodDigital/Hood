using System.Threading.Tasks;
using System;
using Hood.Models;
using Hood.Extensions;
using Hood.Interfaces;
using SendGrid.Helpers.Mail;

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

        public async Task<Response> ProcessAndSend(IEmailSendable model)
        {
            try
            {
                MailObject message = new MailObject();
                message = model.WriteToMailObject(message);

                if (model.SendToRecipient)
                {
                    message.To = model.To;
                    await _email.SendEmailAsync(message, model.From, model.ReplyTo);
                }

                message = new MailObject();
                message = model.WriteNotificationToMailObject(message);

                if (model.NotifyEmails != null)
                {
                    foreach (var recipient in model.NotifyEmails)
                    {
                        message.To = recipient;
                        await _email.SendEmailAsync(message, model.From, model.ReplyTo);
                    }
                }

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