using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using Hood.Models;
using Microsoft.AspNetCore.Hosting;
using SendGrid.Helpers.Mail;

namespace Hood.Services
{
    public class WelcomeEmailSender : IMailSenderService<WelcomeEmailModel>
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ISettingsRepository _settings;
        private readonly IRazorViewRenderer _renderer;
        private readonly IEmailSender _email;
        private readonly IHostingEnvironment _environment;

        public WelcomeEmailSender(IHttpContextAccessor contextAccessor,
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

        public async Task<Models.Response> ProcessAndSend(WelcomeEmailModel model, 
                                                            bool notifySender = true, 
                                                            string notifyRole = "NewAccountNotifications", 
                                                            EmailAddress notifyEmail = null, 
                                                            string NotificationTitle = null, 
                                                            string NotificationMessage = null, 
                                                            string AdminNotificationTitle = null, 
                                                            string AdminNotificationMessage = null)

        {
            try
            {
                AccountSettings accountSettings = _settings.GetAccountSettings();

                if (!accountSettings.EnableWelcome)
                    return new Response(true);

                try
                {
                    MailObject message = new MailObject();
                    if (accountSettings.NotifyNewAccount)
                    {
                        message.PreHeader = _settings.ReplacePlaceholders(accountSettings.WelcomeTitle);
                        message.Subject = _settings.ReplacePlaceholders(accountSettings.WelcomeSubject + " [COPY]");
                        message.AddH1(_settings.ReplacePlaceholders(accountSettings.WelcomeTitle));
                        message.AddDiv(accountSettings.WelcomeMessage);
                        message = model.WriteToMessage(message);

                        if (_environment.IsDevelopment() || _environment.IsStaging())
                        {
                            await _email.NotifyRoleAsync(message, "SuperUser");
                        }
                        else
                        {
                            await _email.NotifyRoleAsync(message, notifyRole);
                        }

                    }

                    message = new MailObject()
                    {
                        To = new SendGrid.Helpers.Mail.EmailAddress(model.User.Email, model.User.FullName),
                        PreHeader = _settings.ReplacePlaceholders(accountSettings.WelcomeSubject),
                        Subject = _settings.ReplacePlaceholders(accountSettings.WelcomeSubject)
                    };
                    message.AddH1(_settings.ReplacePlaceholders(accountSettings.WelcomeTitle));
                    message.AddParagraph(_settings.ReplacePlaceholders(accountSettings.WelcomeMessage));
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