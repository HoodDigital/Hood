using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Hood.Extensions;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

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
