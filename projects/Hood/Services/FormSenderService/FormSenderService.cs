using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Http;
using Hood.Extensions;
using System.Threading.Tasks;
using Hood.Infrastructure;
using System;
using System.Net;
using Hood.Models;

namespace Hood.Services
{
    public class FormSenderService : IFormSenderService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ISettingsRepository _settings;
        private readonly IRazorViewRenderer _renderer;
        private readonly IEmailSender _email;

        public FormSenderService(IHttpContextAccessor contextAccessor,
                              ISettingsRepository site,
                              IRazorViewRenderer renderer,
                              IEmailSender email)
        {
            _contextAccessor = contextAccessor;
            _settings = site;
            _email = email;
            _renderer = renderer;
        }

        public async Task<Models.Response> ProcessContactFormModel(IContactFormModel model)
        {
            try
            {
                ContactSettings contactSettings = _settings.GetContactSettings();

                try
                {
                    MailObject message = new MailObject();

                    string siteEmail = contactSettings.Email;
                    if (!string.IsNullOrEmpty(siteEmail))
                    {
                        message.To = new SendGrid.Helpers.Mail.EmailAddress(siteEmail);
                        message.PreHeader = _settings.ReplacePlaceholders(contactSettings.AdminNoficationSubject);
                        message.Subject = _settings.ReplacePlaceholders(contactSettings.AdminNoficationSubject);
                        message.AddH1(_settings.ReplacePlaceholders(contactSettings.AdminNoficationTitle));
                        message.AddParagraph(_settings.ReplacePlaceholders(contactSettings.AdminNoficationMessage));
                        message.AddParagraph("Name: <strong>" + model.Name + "</strong>");
                        message.AddParagraph("Email: <strong>" + model.Email + "</strong>");
                        message.AddParagraph("Phone: <strong>" + model.PhoneNumber + "</strong>");
                        message.AddParagraph("Subject: <strong>" + model.Subject + "</strong>");
                        message.AddParagraph("Enquiry:");
                        message.AddParagraph("<strong>" + model.Enquiry + "</strong>");
                        await _email.SendEmail(message);
                    }

                    message = new MailObject();
                    message.To = new SendGrid.Helpers.Mail.EmailAddress(model.Email);
                    message.PreHeader = _settings.ReplacePlaceholders(contactSettings.Subject);
                    message.Subject = _settings.ReplacePlaceholders(contactSettings.Subject);
                    message.AddH1(_settings.ReplacePlaceholders(contactSettings.Title));
                    message.AddParagraph(_settings.ReplacePlaceholders(contactSettings.Message));
                    message.AddParagraph("Name: <strong>" + model.Name + "</strong>");
                    message.AddParagraph("Email: <strong>" + model.Email + "</strong>");
                    message.AddParagraph("Phone: <strong>" + model.PhoneNumber + "</strong>");
                    message.AddParagraph("Subject: <strong>" + model.Subject + "</strong>");
                    message.AddParagraph("Enquiry:");
                    message.AddParagraph("<strong>" + model.Enquiry + "</strong>");
                    await _email.SendEmail(message);

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