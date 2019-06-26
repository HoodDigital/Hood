using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using Hood.Models;
using Microsoft.AspNetCore.Hosting;
using SendGrid.Helpers.Mail;
using Hood.Extensions;
using Hood.Core;

namespace Hood.Services
{
    public class MailService : IMailService
    {
        private readonly IEmailSender _email;

        public MailService(IEmailSender email)
        {
            _email = email;
        }

        public async Task<Response> ProcessAndSend(IEmailSendable model)
        {
            try
            {
                ContactSettings contactSettings = Engine.Settings.Contact;

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

                    return new Response(true);
                }
                catch (Exception sendEx)
                {
                    throw new Exception("There was a problem sending the message: " + sendEx.Message, sendEx);
                }

            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }


    }
}