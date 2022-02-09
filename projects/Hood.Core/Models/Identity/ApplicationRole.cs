using Microsoft.AspNetCore.Identity;

namespace Hood.Models
{
    public partial class ApplicationRole : IdentityRole<string>
    {   
        public ApplicationRole() : base()
        { }
        
        public ApplicationRole(string roleName) : base(roleName)
        { }

        public string RemoteId { get; set; }
    }

}
