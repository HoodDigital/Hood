using Hood.Entities;
using Hood.Extensions;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public class ForumAccessEntity : BaseEntity
    {

        [Display(Name = "Viewing Requires Login", Description = "Choose whether or not users need to be logged in order to view the forums.")]
        public bool ViewingRequiresLogin { get; set; }
        [Display(Name = "Viewing Susbcriptions", Description = "Choose which subscriptions users need to be subscribed to in order to view the forums.")]
        public string ViewingSubscriptions { get; set; }
        [Display(Name = "Viewing Roles", Description = "Choose which roles users need to be added to in order to view the forums.")]
        public string ViewingRoles { get; set; }

        [NotMapped]
        [JsonIgnore]
        public List<string> ViewingSubscriptionList
        {
            get
            {
                if (!ViewingSubscriptions.IsSet())
                    return new List<string>();
                return JsonConvert.DeserializeObject<List<string>>(ViewingSubscriptions);
            }
        }
        [NotMapped]
        [JsonIgnore]
        public bool ViewingRequiresSpecificSubscription
        {
            get
            {
                if (ViewingSubscriptionList != null)
                    if (ViewingSubscriptionList.Count > 0)
                        return true;

                return false;
            }
        }

        [Display(Name = "Posting Requires Login", Description = "Choose whether or not users need to be logged in order to post on the forums.")]
        public bool PostingRequiresLogin { get; set; }
        [Display(Name = "Posting Susbcriptions", Description = "Choose which subscriptions users need to be subscribed to in order to post on the forums.")]
        public string PostingSubscriptions { get; set; }
        [Display(Name = "Posting Roles", Description = "Choose which roles users need to be added to in order to post on the forums.")]
        public string PostingRoles { get; set; }

        [NotMapped]
        [JsonIgnore]
        public List<string> PostingSubscriptionList
        {
            get
            {
                if (!PostingSubscriptions.IsSet())
                    return new List<string>();
                return JsonConvert.DeserializeObject<List<string>>(PostingSubscriptions);
            }
        }
        [NotMapped]
        [JsonIgnore]
        public bool PostingRequiresSpecificSubscription
        {
            get
            {
                if (PostingSubscriptionList != null)
                    if (PostingSubscriptionList.Count > 0)
                        return true;
                return false;
            }
        }

        [JsonIgnore]
        public IList<Subscription> Subscriptions { get; set; }
        [JsonIgnore]
        public IList<IdentityRole> Roles { get; set; }
    }
}