using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Hood.ViewModels
{
    public class UserSubscriptionListModel : PagedList<UserSubscription>, IPageableModel
    {
        public UserSubscriptionListModel()
        {
            Linked = true;
            Status = "currently-active";
        }

        public UserSubscriptionListModel(IQueryable<UserSubscription> source, int pageIndex, int pageSize) : base(source, pageIndex, pageSize)
        {
            Linked = true;
            Status = "currently-active";
        }

        public UserSubscriptionListModel(IList<UserSubscription> source, int pageIndex, int pageSize) : base(source, pageIndex, pageSize)
        {
            Linked = true;
            Status = "currently-active";
        }

        [FromQuery(Name = "subscription")]
        public string Subscription { get; set; }
        [FromQuery(Name = "plan")]
        [Display(Name = "Subscription Plan", Description = "Show only the subscriptions to the selected plan.")]
        public int? SubscriptionPlanId { get; set; }
        [FromQuery(Name = "status")]
        public string Status { get; set; }
        [FromQuery(Name = "linked")]
        [Display(Name = "Show only linked subscriptions", Description = "Show only the subscriptions that are connected to active accounts, when accounts are deleted, records of the subscriptions are retained.")]
        public bool Linked { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);
            query += Subscription.IsSet() ? "&subscription=" + Subscription : "";
            query += SubscriptionPlanId.HasValue ? "&plan=" + SubscriptionPlanId : "";
            query += Status.IsSet() ? "&status=" + Status : "";
            query += Linked ? "&linked=true" : "";
            return query;
        }

    }
}