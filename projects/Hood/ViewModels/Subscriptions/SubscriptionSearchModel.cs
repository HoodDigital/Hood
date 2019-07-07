using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Hood.ViewModels
{
    public class SubscriptionSearchModel : PagedList<Subscription>, IPageableModel
    {
        [FromQuery(Name = "category")]
        public string Category { get; set; }
        [FromQuery(Name = "addon")]
        public bool Addon { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            query += Category.IsSet() ? "&category=" + Category : "";
            query += Search.IsSet() ? "&search=" + Search : "";
            query += Order.IsSet() ? "&order=" + Order : "";
            query += Addon ? "&addon=true" : "";
            return query;
        }

    }
}