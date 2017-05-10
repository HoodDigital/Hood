using System;
using System.ComponentModel.DataAnnotations;
using Hood.Services;

namespace Hood.Models
{
    public class ContactFormModel : IContactFormModel
    {
        public string FormId { get; set; }
        public string FormClass { get; set; }
        public string FormAction { get; set; }

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

        public bool ShowValidationMessage { get; set; }
        public bool ShowValidationIndividualMessages { get; set; }

        public ContactFormModel()
        {
        }

        public ContactFormModel(bool showValidationMessage, bool showValidationIndividualMessages)
        {
            ShowValidationMessage = showValidationMessage;
            ShowValidationIndividualMessages = showValidationIndividualMessages;
        }

        public MailObject WriteToMessage(MailObject message)
        {
            message.AddParagraph("Name: <strong>" + this.Name + "</strong>");
            message.AddParagraph("Email: <strong>" + this.Email + "</strong>");
            message.AddParagraph("Phone: <strong>" + this.PhoneNumber + "</strong>");
            message.AddParagraph("Subject: <strong>" + this.Subject + "</strong>");
            message.AddParagraph("Enquiry:");
            message.AddParagraph("<strong>" + this.Enquiry + "</strong>");
            return message;
        }
    }

    public interface IContactFormModel
    {
        string Name { get; set; }
        string Email { get; set; }
        string PhoneNumber { get; set; }
        string Enquiry { get; set; }
        string Message { get; set; }
        string Subject { get; set; }
        bool ShowValidationMessage { get; set; }
        bool ShowValidationIndividualMessages { get; set; }

        MailObject WriteToMessage(MailObject message);
    }
}
