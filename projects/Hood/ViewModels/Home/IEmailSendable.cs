using Hood.Services;

namespace Hood.Models
{
    public interface IEmailSendable
    {
        MailObject WriteToMessage(MailObject message);
    }
}
