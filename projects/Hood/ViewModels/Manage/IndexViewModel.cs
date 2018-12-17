using Hood.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hood.Interfaces;
using Hood.Enums;
using Hood.BaseTypes;

namespace Hood.ViewModels
{
    public class IndexViewModel : SaveableModel
    {
        public string UserId { get; set; }

        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        public string StatusMessage { get; set; }

        public UserProfile Profile { get; set; }

        public IMediaObject Avatar { get; set; }

        public IList<string> Roles { get; set; }
    }
}
