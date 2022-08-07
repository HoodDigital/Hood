using Hood.Extensions;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public class ApplicationUser : IdentityUser<string>, IHoodIdentity
    {
        public string BillingAddressJson { get; set; }
        public string DeliveryAddressJson { get; set; }
        public string AvatarJson { get; set; }
        public UserProfile UserProfile { get; set; }        
        public bool Active { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastLogOn { get; set; }
        public string LastLoginIP { get; set; }
        public string LastLoginLocation { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }


}
