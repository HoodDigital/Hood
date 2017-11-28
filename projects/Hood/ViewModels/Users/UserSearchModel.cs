using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Hood.Models
{
    public class UserSearchModel : PagedList<ApplicationUser>
    {
        public UserSearchModel(IQueryable<ApplicationUser> source, int pageIndex, int pageSize) : base(source, pageIndex, pageSize)
        {
        }
        public UserSearchModel(IList<ApplicationUser> source, int pageIndex, int pageSize) : base(source, pageIndex, pageSize)
        {
        }

        [FromQuery(Name = "sort")]
        public string Order { get; set; }
        [FromQuery(Name = "role")]
        public string Role { get; set; }
        [FromQuery(Name = "search")]
        public string Search { get; set; }
    }
}
