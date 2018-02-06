using Hood.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hood.Interfaces;
using Hood.Enums;

namespace Hood.ViewModels
{
    public class IndexViewModel: ISaveableModel
    {
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

        public string UserId { get; internal set; }

        public AlertType MessageType { get; set; }
        public string SaveMessage { get; set; }

        public IList<string> Roles { get; set; }
    }
}
