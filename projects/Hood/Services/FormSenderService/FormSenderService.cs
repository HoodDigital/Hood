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
        private readonly ISiteConfiguration _site;
        private readonly IRazorViewRenderer _renderer;
        private readonly IEmailSender _email;

        public FormSenderService(IHttpContextAccessor contextAccessor,
                              ISiteConfiguration site,
                              IRazorViewRenderer renderer,
                              IEmailSender email)
        {
            _contextAccessor = contextAccessor;
            _site = site;
            _email = email;
            _renderer = renderer;
        }

        public async Task<Response> ProcessContactFormModel(IContactFormModel model)
        {
            try
            {
                ContactSettings contactSettings = _site.GetContactSettings();

                try
                {
                    MailObject message = new MailObject();

                    string siteEmail = contactSettings.Email;
                    if (!string.IsNullOrEmpty(siteEmail))
                    {
                        message.To = new SendGrid.Helpers.Mail.Email(siteEmail);
                        message.PreHeader = "New enquiry via the " + _site.GetSiteTitle() + " website.";
                        message.Subject = "New enquiry via the " + _site.GetSiteTitle() + " website.";
                        message.AddH1("New enquiry!");
                        message.AddParagraph("A new enquiry has been recieved via " + _site.GetSiteTitle() + " website.");
                        message.AddParagraph("Name: <strong>" + model.Name + "</strong>");
                        message.AddParagraph("Email: <strong>" + model.Email + "</strong>");
                        message.AddParagraph("Phone: <strong>" + model.PhoneNumber + "</strong>");
                        message.AddParagraph("Subject: <strong>" + model.Subject + "</strong>");
                        message.AddParagraph("Enquiry:");
                        message.AddParagraph("<strong>" + model.Enquiry + "</strong>");
                        await _email.SendEmail(message);
                    }

                    string msg = contactSettings.ThankYouMessage;
                    if (string.IsNullOrEmpty(msg))
                        msg += "Thank you for contacting us! Your enquiry has been successfully sent, and we are currently digesting it. We will be in touch once we have had a read. Thanks!";

                    message = new MailObject();
                    message.To = new SendGrid.Helpers.Mail.Email(model.Email);
                    message.PreHeader = "Your enquiry with " + _site.GetSiteTitle();
                    message.Subject = "Your enquiry with " + _site.GetSiteTitle();
                    message.AddH1("Your enquiry has been sent.");
                    message.AddParagraph(msg);
                    message.AddParagraph("Name: <strong>" + model.Name + "</strong>");
                    message.AddParagraph("Email: <strong>" + model.Email + "</strong>");
                    message.AddParagraph("Phone: <strong>" + model.PhoneNumber + "</strong>");
                    message.AddParagraph("Subject: <strong>" + model.Subject + "</strong>");
                    message.AddParagraph("Enquiry:");
                    message.AddParagraph("<strong>" + model.Enquiry + "</strong>");
                    await _email.SendEmail(message);

                    return new Response(true);
                }
                catch (Exception sendEx)
                {
                    throw new Exception("There was a problem sending the message: " + sendEx.Message);
                }

            }
            catch (Exception ex)
            {
                return new Response(ex);
            }
        }
    }
}