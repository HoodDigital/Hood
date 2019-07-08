using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;

namespace Hood.ViewModels
{
    public class UserListModel : PagedList<UserProfile>, IPageableModel
    {
        [FromQuery(Name = "role")]
        public string Role { get; set; }
        [FromQuery(Name = "sub")]
        public string Subscription { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            query += Role.IsSet() ? "&role=" + Role : "";
            return query;
        }
    }
}
