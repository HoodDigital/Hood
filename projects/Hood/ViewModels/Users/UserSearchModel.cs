using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Models
{
    public class UserSearchModel : PagedList<ApplicationUser>
    {
        public string Sort { get; set; }
        public string Role { get; set; }
        public string Search { get; set; }
    }
}
