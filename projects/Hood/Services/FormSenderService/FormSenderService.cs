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
                                                            string NotifyRole = "ContactFormNotifications",
                                                            EmailAddress to = null, 
                                                            EmailAddress from = null,
                                                            string NotificationTitle = null, 
                                                            string NotificationMessage = null,
                                                            string AdminNotificationTitle = null, 
                                                            string AdminNotificationMessage = null)
        {
            try
            {
                ContactSettings contactSettings = _settings.GetContactSettings();

                try
                {
                    MailObject message = new MailObject();

                    string siteEmail = to == null ? contactSettings.Email : to.Email;
                    if (!string.IsNullOrEmpty(siteEmail))
                    {

                        message.PreHeader = _settings.ReplacePlaceholders(model.Subject);
                        message.Subject = _settings.ReplacePlaceholders(model.Subject);
                        message.AddH1(_settings.ReplacePlaceholders(
                            AdminNotificationTitle.IsSet() ? AdminNotificationTitle : contactSettings.AdminNoficationTitle
                        ));
                        message.AddDiv(_settings.ReplacePlaceholders(
                            AdminNotificationMessage.IsSet() ? AdminNotificationMessage : contactSettings.AdminNoficationMessage
                        ));
                        message = model.WriteToMessage(message);


                        if (_environment.IsDevelopment())
                        {
                            await _email.NotifyRoleAsync(message, "SuperUser");
                        }
                        else
                        {
                            message.To = new EmailAddress(siteEmail);
                            await _email.SendEmailAsync(message);
                            await _email.NotifyRoleAsync(message, NotifyRole);
                        }
                    }

                    message = new MailObject()
                    {
                        To = model.To,
                        PreHeader = _settings.ReplacePlaceholders(model.Subject),
                        Subject = _settings.ReplacePlaceholders(model.Subject)
                    };
                    message.AddH1(_settings.ReplacePlaceholders(
                            NotificationTitle.IsSet() ? NotificationTitle : contactSettings.Title
                        ));
                    message.AddDiv(_settings.ReplacePlaceholders(
                            NotificationTitle.IsSet() ? NotificationTitle : contactSettings.Title
                        ));
                    message = model.WriteToMessage(message);
                    await _email.SendEmailAsync(message);

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