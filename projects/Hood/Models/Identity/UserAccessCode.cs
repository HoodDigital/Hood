using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hood.Models
{
    public class UserAccessCode
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; }
        public DateTime Expiry { get; set; }
        public string Type { get; set; }
        public bool Used { get; set; }
        public DateTime DateUsed { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
