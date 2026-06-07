using System;
using Microsoft.AspNetCore.Identity;

namespace Hood.Models
{
    public class ApplicationUser : IdentityUser<string>, IHoodIdentity
    {
        public ApplicationUser()
        {
            // The generic IdentityUser<TKey> base does not generate an Id (only the
            // non-generic IdentityUser does), so without this every creation site
            // hands EF a null string primary key and tracking fails (HOOD-93).
            Id = Guid.NewGuid().ToString();
        }

        public UserProfile UserProfile { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastLogOn { get; set; }
        public string LastLoginIP { get; set; }
        public string LastLoginLocation { get; set; }
    }
}
