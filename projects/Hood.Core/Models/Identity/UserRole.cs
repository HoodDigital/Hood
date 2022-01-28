using Microsoft.AspNetCore.Identity;

namespace Hood.Models
{
    public class UserRole : IdentityRole<string>
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
    }
}