using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Hood.Models
{
    public class IndexViewModel
    {
        public ApplicationUser User { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }

        public IList<string> Roles { get; set; }

        public string PhoneNumber { get; set; }

        public bool TwoFactor { get; set; }

        public bool BrowserRemembered { get; set; }
    }
}
