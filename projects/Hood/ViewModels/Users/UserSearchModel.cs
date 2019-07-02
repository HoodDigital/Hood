using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;

namespace Hood.ViewModels
{
    public class UserSearchModel : PagedList<ApplicationUser>, IPageableModel
    {
        [FromQuery(Name = "sort")]
        public string Order { get; set; }
        [FromQuery(Name = "role")]
        public string Role { get; set; }
        [FromQuery(Name = "search")]
        public string Search { get; set; }

        public string GetPageUrl(int pageIndex)
        {
            var query = string.Format("?page={0}&pageSize={1}", pageIndex, PageSize);
            query += Search.IsSet() ? "&search=" + Search : "";
            query += Role.IsSet() ? "&role=" + Role : "";
            query += Order.IsSet() ? "&sort=" + Order : "";
            return query;
        }
    }
}
