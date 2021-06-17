using Hood.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public partial class UserAccessCode : BaseEntity
    { 
        public string Code { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm}")]
        public DateTime Expiry { get; set; }
        public string Type { get; set; }
        public bool Used { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm}")]
        public DateTime DateUsed { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
