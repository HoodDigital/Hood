using Hood.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hood.Interfaces;
using Hood.BaseTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hood.ViewModels
{
    public class UserViewModel : SaveableModel, ISaveableModel
    {
        public string LocalUserId { get; set; }

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

        public IList<IdentityRole> AllRoles { get; set; }

        [FromQuery(Name = "created")]
        public bool NewAccountCreated { get; set; }
        public IList<Auth0Identity> Accounts { get; set; }

        [FromQuery(Name = "returnUrl")]
        public string ReturnUrl { get; set; }
    }
}
