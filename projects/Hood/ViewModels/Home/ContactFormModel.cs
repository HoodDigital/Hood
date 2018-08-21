using System.ComponentModel.DataAnnotations;
using Hood.Services;
using Hood.ViewModels;

namespace Hood.Models
{
    public class ContactFormModel : HoneyPotFormModel, IContactFormModel
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
            message.AddParagraph("Name: <strong>" + Name + "</strong>");
            message.AddParagraph("Email: <strong>" + Email + "</strong>");
            message.AddParagraph("Phone: <strong>" + PhoneNumber + "</strong>");
            message.AddParagraph("Subject: <strong>" + Subject + "</strong>");
            message.AddParagraph("Enquiry:");
            message.AddParagraph("<strong>" + Enquiry + "</strong>");
            return message;
        }
    }

    public interface IContactFormModel : IEmailSendable
    {
        string Name { get; set; }
        string Email { get; set; }
        string Message { get; set; }
        string Subject { get; set; }
        string PhoneNumber { get; set; }
        string Enquiry { get; set; }
        bool ShowValidationMessage { get; set; }
        bool ShowValidationIndividualMessages { get; set; }
    }

    public interface IEmailSendable
    {
        MailObject WriteToMessage(MailObject message);
    }
}
