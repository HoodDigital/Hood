using Hood.BaseTypes;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    [Serializable]
    public class ForumSettings : ForumAccessEntity
    {
        public ForumSettings()
        {
        }

        [Display(Name = "Enable Forums")]
        public bool Enabled { get; set; }
    }

}
