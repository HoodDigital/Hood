using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class SubscriptionPlanListModel : PagedList<SubscriptionPlan>, IPageableModel
    {
        [Display(Name = "Group", Description = "Show only subscriptions from the selected group.")]
        [FromQuery(Name = "group")]
        public int? GroupId { get; set; }
        [FromQuery(Name = "addon")]
        public bool Addon { get; set; }

        public List<SubscriptionGroup> SubscriptionGroups { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            query += GroupId.HasValue ? "&group=" + GroupId.Value : "";
            query += Addon ? "&addon=true" : "";
            return query;
        }

    }
}