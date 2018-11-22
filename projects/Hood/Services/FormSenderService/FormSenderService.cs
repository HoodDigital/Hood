using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using Hood.Models;
using Microsoft.AspNetCore.Hosting;
using SendGrid.Helpers.Mail;
using Hood.Extensions;

namespace Hood.Services
{
    public class FormSenderService : IMailSenderService<IContactFormModel>
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ISettingsRepository _settings;
        private readonly IRazorViewRenderer _renderer;
        private readonly IEmailSender _email;
        private readonly IHostingEnvironment _environment;

        public FormSenderService(IHttpContextAccessor contextAccessor,
                              ISettingsRepository site,
                              IRazorViewRenderer renderer,
                              IEmailSender email,
                              IHostingEnvironment env)
        {
            _contextAccessor = contextAccessor;
            _settings = site;
            _email = email;
            _environment = env;
            _renderer = renderer;
        }

        public async Task<Models.Response> ProcessAndSend(IContactFormModel model,
                                                            bool notifySender = true,
                                                            string notifyRole = "ContactFormNotifications",
                                                            EmailAddress notifyEmail = null,
                                                            string notificationTitle = null,
                                                            string notificationMessage = null,
                                                            string adminNotificationTitle = null,
                                                            string adminNotificationMessage = null)
        {
            try
            {
                ContactSettings contactSettings = _settings.GetContactSettings();

                try
                {
                    MailObject message = new MailObject();

                    // Send to notification email and/or notification roles.

                    message.PreHeader = _settings.ReplacePlaceholders(model.Subject);
                    message.Subject = _settings.ReplacePlaceholders(model.Subject);
                    message.AddH1(_settings.ReplacePlaceholders(
                        adminNotificationTitle.IsSet() ? adminNotificationTitle : contactSettings.AdminNoficationTitle
                    ));
                    message.AddParagraph(_settings.ReplacePlaceholders(
                        adminNotificationMessage.IsSet() ? adminNotificationMessage : contactSettings.AdminNoficationMessage
                    ));
                    message = model.WriteToMessage(message);

                    if (_environment.IsDevelopment())
                    {
                        await _email.NotifyRoleAsync(message, "SuperUser");
                    }
                    else
                    {
                        message.To = notifyEmail == null ? new EmailAddress(contactSettings.Email) : notifyEmail;
                        await _email.SendEmailAsync(message);

                        if (notifyRole.IsSet())
                            await _email.NotifyRoleAsync(message, notifyRole);
                    }

                    if (notifySender)
                    {
                        // Now recreate the message and send to the sender as a notification of their send success.

                        message = new MailObject()
                        {
                            To = model.To,
                            PreHeader = _settings.ReplacePlaceholders(model.Subject),
                            Subject = _settings.ReplacePlaceholders(model.Subject)
                        };
                        message.AddH1(_settings.ReplacePlaceholders(
                                notificationTitle.IsSet() ? notificationTitle : contactSettings.Title
                            ));
                        message.AddParagraph(_settings.ReplacePlaceholders(
                                notificationMessage.IsSet() ? notificationMessage : contactSettings.Message
                            ));

                        message = model.WriteToMessage(message);
                        await _email.SendEmailAsync(message);
                    }

                    return new Models.Response(true);
                }
                catch (Exception sendEx)
                {
                    throw new Exception("There was a problem sending the message: " + sendEx.Message);
                }

            }
            catch (Exception ex)
            {
                return new Models.Response(ex);
            }
        }


    }
}