using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Hood.Interfaces;
using Hood.Extensions;
using Hood.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public partial class Subscription :  BaseEntity, ISaveableModel
    {
        [Display(Name = "Code",
                Description = "Used as a unique id for the subscription. Cannot be changed once set.")]
        public string StripeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [Display(Name = "Subscription Category",
            Description = "A user can only subscribe to one of each category. Think of categories as products, with different subscription levels.")]
        public string Category { get; set; }
        [Display(Name = "Public",
               Description = "Display this as a public subscription (User can up/downgrade to it)")]
        public bool Public { get; set; }

        [Display(Name = "Level",
                 Description = "This is the level of the subscription, higher numbers indicate a higher access level, subscriptions are ordered by level.")]
        public int Level { get; set; }

        [Display(Name = "Add On",
               Description = "Addons are excluded from the level based subscriptions, and can be bolted onto accounts in addition to regular subscriptions.")]
        public bool Addon { get; set; }
        public int NumberAllowed { get; set; }

        // Stripe Fields
        [Display(Name = "Price",
            Description = "The price in your chosen currency for the subscription, per interval. Cannot be changed once set.")]
        public int Amount { get; set; }
        public DateTime Created { get; set; }
        public string Currency { get; set; }
        [Display(Name = "Charge Interval",
             Description = "The time period in which the subscription cycle is measured.")]
        public string Interval { get; set; }
        [Display(Name = "Interval Count",
                Description = "How many intervals between charges.")]
        public int IntervalCount { get; set; }
        public bool LiveMode { get; set; }
        public string StatementDescriptor { get; set; }
        [Display(Name = "Trial Period (Days)",
               Description = "If entered, a trial will be active before charging for the set number of days.")]
        public int? TrialPeriodDays { get; set; }

        public List<UserSubscription> Users { get; set; }

        [NotMapped]
        public int SubscriberCount
        {
            get
            {
                if (Users == null)
                    return 0;
                else
                    return Users.Count;
            }
        }
        [NotMapped]
        public int ActiveSubscribers
        {
            get
            {
                if (Users == null)
                    return 0;
                else
                    return Users.Where(u => u.Status == "active" || u.Status == "trialing").Count();
            }
        }
        [NotMapped]
        public string Price
        {
            get
            {
                return ((double)Amount / 100).ToString("C");
            }
        }
        [NotMapped]
        public string FullPrice
        {
            get
            {
                return ((double)Amount / 100).ToString("C") + " every " + IntervalCount + " " + Interval + "(s)";
            }
        }

        // Featured Images
        public string FeaturedImageUrl { get; set; }

        // Creator/Editor
        public string CreatedBy { get; set; }
        public DateTime LastEditedOn { get; set; }
        public string LastEditedBy { get; set; }

        public IList<SubscriptionFeature> Features { get; set; }

        public SubscriptionFeature GetFeature(string name)
        {
            SubscriptionFeature cm = Features.FirstOrDefault(p => p.Name == name);
            if (cm == null)
                return new SubscriptionFeature()
                {
                    BaseValue = null,
                    Name = name,
                    Type = null
                };
            return cm;
        }

        public void UpdateFeature<T>(string name, T value)
        {
            SubscriptionFeature cm = Features.FirstOrDefault(p => p.Name == name);
            if (cm != null)
            {
                cm.Set(value);
            }
        }

        public bool HasMeta(string name)
        {
            SubscriptionFeature cm = Features.FirstOrDefault(p => p.Name == name);
            if (cm == null)
                return false;
            return true;
        }
    }
}

