using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class UserNote
    {
        public Guid Id { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm}")]
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string Note { get; set; }
    }
}
