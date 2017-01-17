using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class MailchimpFormModel
    {
        [Required]
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
