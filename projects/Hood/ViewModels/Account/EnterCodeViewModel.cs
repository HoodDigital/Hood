using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class EnterCodeViewModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
