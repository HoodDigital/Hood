using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Hood.Interfaces;

namespace Hood.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public partial class Subscription
    {
        // Content
        [Key]
        public int Id { get; set; }
        public string StripeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Colour { get; set; }
        public bool Public { get; set; }
        public int Level { get; set; }

        public bool Addon { get; set; }
        public int NumberAllowed { get; set; }

        // Stripe Fields
        public int Amount { get; set; }
        public DateTime Created { get; set; }
        public string Currency { get; set; }
        public string Interval { get; set; }
        public int IntervalCount { get; set; }
        public bool LiveMode { get; set; }
        public string StatementDescriptor { get; set; }
        public int? TrialPeriodDays { get; set; }

        public int SubscriberCount {
            get
            {
                if (Users == null)
                    return 0;
                else
                    return Users.Count;
            }
        }
        public string Price
        {
            get
            {
                return ((double)Amount / 100).ToString("C");
            }
        }
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

        public List<UserSubscription> Users { get; set; }

    }
}

