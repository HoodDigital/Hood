using Microsoft.AspNetCore.Identity;

namespace Hood.Models
{
    public partial class ApplicationRole : IdentityRole<string>
    {   
        public string RemoteId { get; set; }
    }


}
