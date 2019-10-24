using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class SubscriptionPlanListModel : PagedList<SubscriptionPlan>, IPageableModel
    {
        [Display(Name = "Product", Description = "Show only subscriptions from the selected product group.")]
        [FromQuery(Name = "product")]
        public int? ProductId { get; set; }
        [FromQuery(Name = "addon")]
        public bool Addon { get; set; }

        public List<SubscriptionProduct> SubscriptionGroups { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            query += ProductId.HasValue ? "&product=" + ProductId.Value : "";
            query += Addon ? "&addon=true" : "";
            return query;
        }

    }
}