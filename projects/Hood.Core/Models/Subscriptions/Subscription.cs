﻿using Hood.Core;
using Hood.Entities;
using Hood.Extensions;
using Hood.Interfaces;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hood.Models
{
    public class ConnectedStripeProduct : Stripe.Product
    {
        public ConnectedStripeProduct(Product sp)
        {
            sp.CopyProperties(this);
        }

        [NotMapped]
        public SubscriptionProduct SubscriptionProduct { get; set; }
        public int? SubscriptionProductId { get; set; }
    }

    public class ConnectedStripePlan : Stripe.Plan
    {
        public ConnectedStripePlan(Plan sp)
        {
            sp.CopyProperties(this);
        }

        [NotMapped]
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public int? SubscriptionPlanId { get; set; }
    }

    public class SubscriptionPlan : SubscriptionPlanBase
    {
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int TrialCount { get; set; }
        public int InactiveCount { get; set; }
    }

    public sealed class Subscription : SubscriptionPlanBase
    {
        [NotMapped]
        public Plan StripePlan { get; set; }
    }

    public abstract class SubscriptionPlanBase : BaseEntity, ISaveableModel
    {
        [Display(Name = "Code", Description = "Used as a unique id for the subscription. Cannot be changed once set.")]
        public string StripeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [Display(Name = "Subscription Colour", Description = "Used when colouring the subscriptions when displayed in the subscription tables in choose subscription or new subscription pages.")]
        public string Colour { get; set; }
        [Display(Name = "Published", Description = "Display this subscription. Users will be able to subscribe from choose subscription or new subscription pages.")]
        public bool Public { get; set; }

        [Display(Name = "Level", Description = "This is the level of the subscription, higher numbers indicate a higher access level, subscriptions are ordered by level.")]
        public int Level { get; set; }

        [Display(Name = "Add On", Description = "Addons are excluded from the level based subscriptions, and can be bolted onto accounts in addition to regular subscriptions.")]
        public bool Addon { get; set; }
        public int NumberAllowed { get; set; }

        // Stripe Fields
        [Display(Name = "Price", Description = "The price in your chosen currency for the subscription, per interval. Cannot be changed once set.")]
        public int Amount { get; set; }
        public DateTime Created { get; set; }
        [Display(Name = "Currency", Description = "Choose the currency to use.")]
        public string Currency { get; set; }
        [Display(Name = "Charge Interval", Description = "How often to charge the subscriber.")]
        public string Interval { get; set; }
        [Display(Name = "Interval Count", Description = "How many intervals between charges.")]
        public int IntervalCount { get; set; }
        public bool LiveMode { get; set; }
        public string StatementDescriptor { get; set; }
        [Display(Name = "Trial Period (Days)", Description = "If entered, a trial will be active before charging for the set number of days.")]
        public int? TrialPeriodDays { get; set; }

        [NotMapped]
        [Display(Name = "Price", Description = "Price, charged every interval.")]
        public decimal CreatePrice { get; set; }

        [JsonIgnore]
        public List<UserSubscription> Users { get; set; }

        [NotMapped]
        public string Price
        {
            get
            {
                BillingSettings billing = Engine.Settings.Billing;
                if (billing != null)
                {
                    switch (billing.StripeCurrency)
                    {
                        case "gbp":
                            return $"£{Amount.ToCurrencyString()}";
                        case "usd":
                            return $"${Amount.ToCurrencyString()}";
                        case "eur":
                            return $"€{Amount.ToCurrencyString()}";
                    }
                }
                return Amount.ToCurrencyString();
            }
        }
        [NotMapped]
        public string FullPrice => $"{Price} every {IntervalCount} {Interval} (s)";

        // Featured Images
        /// <summary>
        /// Restricted field, used for JSON FeaturedImage.
        /// </summary>
        public string FeaturedImageJson { get; set; }
        [NotMapped]
        public IMediaObject FeaturedImage
        {
            get => FeaturedImageJson.IsSet() ? JsonConvert.DeserializeObject<ContentMedia>(FeaturedImageJson) : MediaBase.Blank;
            set => FeaturedImageJson = JsonConvert.SerializeObject(value);
        }

        // Creator/Editor
        public string CreatedBy { get; set; }
        public DateTime LastEditedOn { get; set; }
        public string LastEditedBy { get; set; }

        [Display(Name = "Subscription Product", Description = "Choose a product group for this plan, if you leave it unset, a product group will be generated.")]
        public int? SubscriptionProductId { get; set; }
        public SubscriptionProduct SubscriptionProduct { get; set; }

        /// <summary>
        /// Restricted field, used for JSON features.
        /// </summary>
        public string FeaturesJson { get; set; }
        [NotMapped]
        public List<SubscriptionFeature> Features
        {
            get => FeaturesJson.IsSet() ? JsonConvert.DeserializeObject<List<SubscriptionFeature>>(FeaturesJson) : new List<SubscriptionFeature>();
            set => FeaturesJson = JsonConvert.SerializeObject(value);
        }
    }
}
