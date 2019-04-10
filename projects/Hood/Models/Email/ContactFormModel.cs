using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hood.Core;
using Hood.Extensions;
using Hood.Services;
using Hood.ViewModels;
using SendGrid.Helpers.Mail;

namespace Hood.Models
{
    public class ContactFormModel : HoneyPotFormModel, IEmailSendable
    {
        public EmailAddress From { get; set; } = null;

        [Required]
        [Display(Name = "Your name")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "E-mail Address")]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Enquiry")]
        public string Enquiry { get; set; }

        public string Message { get; set; }

        public string Subject { get; set; }

        public EmailAddress To
        {
            get
            {
                return new EmailAddress(Email, Name);
            }
        }

        public bool ShowValidationMessage { get; set; }
        public bool ShowValidationIndividualMessages { get; set; }

        public string AdminNotificationTitle { get; set; }
        public string AdminNotificationSubject { get; set; }
        public string AdminNotificationMessage { get; set; }

        public string NotificationTitle { get; set; }
        public string NotificationSubject { get; set; }
        public string NotificationMessage { get; set; }

        public bool SendToRecipient { get; set; }
        public List<EmailAddress> NotifyEmails { get; set; }
        public string NotifyRole { get; set; }

        public ContactFormModel()
        {
        }

        public ContactFormModel(bool showValidationMessage, bool showValidationIndividualMessages)
        {
            ShowValidationMessage = showValidationMessage;
            ShowValidationIndividualMessages = showValidationIndividualMessages;
        }

        public MailObject WriteToMailObject(MailObject message)
        {
            var settings = Engine.Current.Resolve<ISettingsRepository>();
            var contactSettings = settings.GetContactSettings();

            message.PreHeader = settings.ReplacePlaceholders(
                NotificationSubject.IsSet() ? NotificationSubject : contactSettings.Title
            );
            message.Subject = settings.ReplacePlaceholders(
                NotificationSubject.IsSet() ? NotificationSubject : contactSettings.Subject
            );
            message.AddH1(settings.ReplacePlaceholders(
                NotificationTitle.IsSet() ? NotificationTitle : contactSettings.Title
            ));
            message.AddParagraph(settings.ReplacePlaceholders(
                NotificationMessage.IsSet() ? NotificationMessage : contactSettings.Message
            ));
            message.AddParagraph("Name: <strong>" + Name + "</strong>");
            message.AddParagraph("Email: <strong>" + Email + "</strong>");
            message.AddParagraph("Phone: <strong>" + PhoneNumber + "</strong>");
            message.AddParagraph("Subject: <strong>" + Subject + "</strong>");
            message.AddParagraph("Enquiry:");
            message.AddParagraph("<strong>" + Enquiry + "</strong>");
            return message;
        }


        public MailObject WriteNotificationToMailObject(MailObject message)
        {
            var settings = Engine.Current.Resolve<ISettingsRepository>();
            var contactSettings = settings.GetContactSettings();

            message.PreHeader = settings.ReplacePlaceholders(
                AdminNotificationSubject.IsSet() ? AdminNotificationSubject : contactSettings.AdminNoficationSubject
            );
            message.Subject = settings.ReplacePlaceholders(
                AdminNotificationSubject.IsSet() ? AdminNotificationSubject : contactSettings.AdminNoficationSubject
            );
            message.AddH1(settings.ReplacePlaceholders(
                AdminNotificationTitle.IsSet() ? AdminNotificationTitle : contactSettings.AdminNoficationTitle
            ));
            message.AddParagraph(settings.ReplacePlaceholders(
                AdminNotificationMessage.IsSet() ? AdminNotificationMessage : contactSettings.AdminNoficationMessage
            ));
            message.AddParagraph("Name: <strong>" + Name + "</strong>");
            message.AddParagraph("Email: <strong>" + Email + "</strong>");
            message.AddParagraph("Phone: <strong>" + PhoneNumber + "</strong>");
            message.AddParagraph("Subject: <strong>" + Subject + "</strong>");
            message.AddParagraph("Enquiry:");
            message.AddParagraph("<strong>" + Enquiry + "</strong>");
            return message;
        }
    }
}
