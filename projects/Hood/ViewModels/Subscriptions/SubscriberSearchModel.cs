using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class UserSubscriptionListModel : PagedList<ApplicationUser>, IPageableModel
    {
        [FromQuery(Name = "subscription")]
        public string Subscription { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            query += Subscription.IsSet() ? "&subscription=" + Subscription : "";
            return query;
        }

    }
}