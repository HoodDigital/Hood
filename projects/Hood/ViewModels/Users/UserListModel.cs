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
        [FromQuery(Name = "roles")]
        public List<string> RoleIds { get; set; }
        [FromQuery(Name = "subs")]
        public List<string> SubscriptionIds { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);

            query += Role.IsSet() ? "&role=" + Role : "";
            if (RoleIds != null)
                foreach (var roleId in RoleIds)
                    query += "&roles=" + roleId;

            query += Subscription.IsSet() ? "&sub=" + Role : "";
            if (SubscriptionIds != null)
                foreach (var subId in SubscriptionIds)
                    query += "&subs=" + subId;

            return query;
        }
    }
}
