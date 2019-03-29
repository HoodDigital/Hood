using Hood.BaseTypes;
using Hood.Extensions;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    public class EditUserModel : SaveableModel
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; }
        public IList<IdentityRole> AllRoles { get; set; }
    }

    public class SaveProfileModel : SaveableModel
    {
        public UserProfile User { get; set; }
    }

    public class UserProfile : IUserProfile
    {
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [Display(Name = "Company name")]
        public string CompanyName { get; set; }

        [Display(Name = "Display name")]
        public string DisplayName { get; set; }

        public string Phone { get; set; }

        public string Mobile { get; set; }

        [Display(Name = "Twitter URL")]
        public string Twitter { get; set; }

        [Display(Name = "Twitter Handle (@yourname)")]
        public string TwitterHandle { get; set; }

        [Display(Name = "Facebook URL")]
        public string Facebook { get; set; }

        [Display(Name = "Google+ URL")]
        public string GooglePlus { get; set; }

        [Display(Name = "LinkedIn URL")]
        public string LinkedIn { get; set; }

        public bool Anonymous { get; set; }

        public string Bio { get; set; }

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Display(Name = "Website URL")]
        public string WebsiteUrl { get; set; }

        [Display(Name = "Forum Signature")]
        public string ForumSignature { get; set; }

        public string Notes { get; set; }

        public Dictionary<string, string> UserVariables { get; set; }

        public void SetProfile(IUserProfile profile)
        {
            profile.CopyProperties(this);
        }
    }

}