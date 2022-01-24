using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hood.Core;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using SendGrid.Helpers.Mail;

namespace Hood.ViewModels
{
    public class ContactFormModel : SpamPreventionModel, IEmailSendable
    {
        public EmailAddress From { get; set; } = null;
        public EmailAddress ReplyTo { get; set; } = null;

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
        public string NotificationPreHeader { get; private set; }
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
            var contactSettings = Engine.Settings.Contact;

            message.PreHeader = NotificationTitle.IsSet() ? NotificationTitle.ReplaceSiteVariables() : contactSettings.Title.ReplaceSiteVariables();
            message.Subject = NotificationSubject.IsSet() ? NotificationSubject.ReplaceSiteVariables() : contactSettings.Subject.ReplaceSiteVariables();
            message.AddParagraph(NotificationMessage.IsSet() ? NotificationMessage.ReplaceSiteVariables() : contactSettings.Message.ReplaceSiteVariables());
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
            var contactSettings = Engine.Settings.Contact;
            message.PreHeader = AdminNotificationTitle.IsSet() ? AdminNotificationTitle.ReplaceSiteVariables() : contactSettings.AdminNoficationTitle.ReplaceSiteVariables();
            message.Subject = AdminNotificationSubject.IsSet() ? AdminNotificationSubject.ReplaceSiteVariables() : contactSettings.AdminNoficationSubject.ReplaceSiteVariables();
            message.AddParagraph(AdminNotificationMessage.IsSet() ? AdminNotificationMessage.ReplaceSiteVariables() : contactSettings.AdminNoficationMessage.ReplaceSiteVariables());
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
