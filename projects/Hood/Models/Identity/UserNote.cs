using System;

namespace Hood.Models
{
    public class UserNote
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string Note { get; set; }
    }
}
