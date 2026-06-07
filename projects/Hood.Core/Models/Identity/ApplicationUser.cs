using System;
using Microsoft.AspNetCore.Identity;

namespace Hood.Models
{
    public class ApplicationUser : IdentityUser<string>, IHoodIdentity
    {
        public UserProfile UserProfile { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastLogOn { get; set; }
        public string LastLoginIP { get; set; }
        public string LastLoginLocation { get; set; }
    }
}
