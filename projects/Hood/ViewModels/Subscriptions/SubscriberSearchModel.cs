using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class SubscriberSearchModel : PagedList<ApplicationUser>, IPageableModel
    {
        [FromQuery(Name = "subscription")]
        public string Subscription { get; set; }
        [FromQuery(Name = "sort")]
        public string Order { get; set; }
        [FromQuery(Name = "search")]
        public string Search { get; set; }

        public string GetPageUrl(int pageIndex)
        {
            var query = string.Format("?page={0}&pageSize={1}", pageIndex, PageSize);
            query += Search.IsSet() ? "&search=" + Search : "";
            query += Subscription.IsSet() ? "&subscription=" + Subscription : "";
            query += Order.IsSet() ? "&sort=" + Order : "";
            return query;
        }

    }
}