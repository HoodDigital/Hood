using Hood.Entities;
using System;

namespace Hood.Models
{
    public partial class UserAccessCode : UserAccessCode<HoodIdentityUser> { }
    public class UserAccessCode<TUser> : BaseEntity<TUser> where TUser : IHoodUser
    {
        public string Code { get; set; }
        public DateTime Expiry { get; set; }
        public string Type { get; set; }
        public bool Used { get; set; }
        public DateTime DateUsed { get; set; }
    }
}
