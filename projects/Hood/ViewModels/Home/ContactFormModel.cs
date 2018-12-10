using System.ComponentModel.DataAnnotations;
using Hood.Services;
using Hood.ViewModels;
using SendGrid.Helpers.Mail;

namespace Hood.Models
{
    public class ContactFormModel : HoneyPotFormModel, IContactFormModel
    {
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
}
