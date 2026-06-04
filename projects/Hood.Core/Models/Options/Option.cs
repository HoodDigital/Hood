using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class Option
    {
        [Key]
        public string Id { get; set; }
        public string Value { get; set; }
    }
}
