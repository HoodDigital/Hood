using Hood.BaseTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    [Serializable]
    public class ForumSettings : SaveableModel
    {
        public ForumSettings()
        {
        }

        [Display(Name = "Enable Forums")]
        public bool Enabled { get; set; }

        [Display(Name = "Viewing Requires Login")]
        public bool ViewingRequiresLogin { get; set; }
        [Display(Name = "Viewing Requires Subscription")]
        public bool ViewingRequiresSubscription { get; set; }
        [Display(Name = "Viewing Susbcriptions")]
        public string ViewingSusbcriptions { get; set; }

        [Display(Name = "Posting Requires Login")]
        public bool PostingRequiresLogin { get; set; }
        [Display(Name = "Posting Requires Subscription")]
        public bool PostingRequiresSubscription { get; set; }
        [Display(Name = "Posting Susbcriptions")]
        public string PostingSusbcriptions { get; set; }

        [JsonIgnore]
        public List<Subscription> Subscriptions { get; set; }
    }

}
