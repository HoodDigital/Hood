using SendGrid.Helpers.Mail;

namespace Hood.Models
{
    public interface IContactFormModel : IEmailSendable
    {
        EmailAddress To { get; }
        string Subject { get; }
        bool ShowValidationMessage { get; set; }
        bool ShowValidationIndividualMessages { get; set; }
    }
}
