using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class StripePlanListModel : PagedList<ConnectedStripePlan>, IPageableModel
    {
        [Display(Name = "Group", Description = "Show only subscriptions from the selected group.")]
        [FromQuery(Name = "group")]
        public int? GroupId { get; set; }

        [FromQuery(Name = "linked")]
        [Display(Name = "Show only linked subscriptions", Description = "Show only the subscriptions that are connected to active accounts, when accounts are deleted, records of the subscriptions are retained.")]
        public bool Linked { get; set; }

        public List<SubscriptionGroup> SubscriptionGroups { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            query += GroupId.HasValue ? "&group=" + GroupId.Value : "";
            query += Linked ? "&linked=true" : "";
            return query;
        }
    }
}