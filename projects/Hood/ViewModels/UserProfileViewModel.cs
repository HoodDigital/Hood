using Hood.BaseTypes;
using Hood.Enums;
using Hood.Extensions;
using Hood.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class UserProfileViewModel : IUserProfile
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Bio { get; set; }
        public string CompanyName { get; set; }
        public string Facebook { get; set; }
        public string Instagram { get; set; }
        public string JobTitle { get; set; }
        public string LinkedIn { get; set; }
        public string Twitter { get; set; }
        public string TwitterHandle { get; set; }
        public string WebsiteUrl { get; set; }
        public bool Anonymous { get; set; }
        public string FullName { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

}