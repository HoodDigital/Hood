using Hood.Entities;
using Newtonsoft.Json;
using System;

namespace Hood.Models
{
    public partial class UserAccessCode : BaseEntity
    { 
        public string Code { get; set; }
        public DateTime Expiry { get; set; }
        public string Type { get; set; }
        public bool Used { get; set; }
        public DateTime DateUsed { get; set; }
        public string UserId { get; set; }

        [JsonConverter(typeof(ApplicationUserJsonConverter))]
        public ApplicationUser User { get; set; }
    }
}
