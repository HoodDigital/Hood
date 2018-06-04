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

        [Display(Name = "Viewing Requires Login")]
        public bool ViewingRequiresLogin { get; set; }
        [Display(Name = "Viewing Requires Subscription")]
        public bool ViewingRequiresSubscription { get; set; }
        [Display(Name = "Viewing Susbcriptions")]
        public string ViewingSubscriptions { get; set; }
        [Display(Name = "Viewing Roles")]
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

        [Display(Name = "Posting Requires Login")]
        public bool PostingRequiresLogin { get; set; }
        [Display(Name = "Posting Requires Subscription")]
        public bool PostingRequiresSubscription { get; set; }
        [Display(Name = "Posting Susbcriptions")]
        public string PostingSubscriptions { get; set; }
        [Display(Name = "Posting Roles")]
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