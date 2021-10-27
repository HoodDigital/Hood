using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using System.ComponentModel.DataAnnotations;

namespace Hood.ViewModels
{
    public class UserListModel : PagedList<UserProfile>, IPageableModel, IUserListModel
    {
        [FromQuery(Name = "role")]
        public string Role { get; set; }

        [FromQuery(Name = "unused")]
        [Display(Name = "Unused Accounts", Description = "Accounts which do not have last log in infomation saved.")]
        public bool Unused { get; set; }

        [FromQuery(Name = "inactive")]
        [Display(Name = "Inactive Only")]
        public bool Inactive { get; set; }

        [FromQuery(Name = "active")]
        [Display(Name = "Active Only")]
        public bool Active { get; set; }

        [FromQuery(Name = "phone")]
        [Display(Name = "Unconfirmed Phone Numbers Only")]
        public bool PhoneUnconfirmed { get; set; }

        [FromQuery(Name = "email")]
        [Display(Name = "Unconfirmed Emails Only")]
        public bool EmailUnconfirmed { get; set; }

        [FromQuery(Name = "sub")]
        public string Subscription { get; set; }
        [FromQuery(Name = "roles")]
        public List<string> RoleIds { get; set; }
        [FromQuery(Name = "subs")]
        public List<string> SubscriptionIds { get; set; }

        public override string GetPageUrl(int pageIndex)
        {
            var query = base.GetPageUrl(pageIndex);

            query += Active ? "&active=true" : "";
            query += Inactive ? "&inactive=true" : "";
            query += EmailUnconfirmed ? "&email=true" : "";
            query += PhoneUnconfirmed ? "&phone=true" : "";
            query += Unused ? "&unused=true" : "";

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
