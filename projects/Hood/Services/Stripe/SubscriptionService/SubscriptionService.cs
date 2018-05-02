
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Hood.Models;

namespace Hood.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private IStripeService _stripe;
        private UserManager<ApplicationUser> _userManager;

        public SubscriptionService(IStripeService stripe,
                                   UserManager<ApplicationUser> userManager)
        {
            _stripe = stripe;
            _userManager = userManager;
        }

        public async Task<StripeSubscription> CancelSubscriptionAsync(string customerId, string subscriptionId, bool cancelAtPeriodEnd = true)
        {
            return await _stripe.SubscriptionService.CancelAsync(subscriptionId, cancelAtPeriodEnd); // optional cancelAtPeriodEnd flag        
        }

        public async Task<StripeSubscription> FindById(string customerId, string subscriptionId)
        {
            return await _stripe.SubscriptionService.GetAsync(subscriptionId);
        }

        public async Task<StripeSubscription> SubscribeUserAsync(string customerId, string planId, DateTime? trialEnd = null, decimal taxPercent = 0)
        {
            var options = new StripeSubscriptionCreateOptions()
            {
                TrialEnd = trialEnd,
                TaxPercent = taxPercent
            };
            StripeSubscription stripeSubscription = await _stripe.SubscriptionService.CreateAsync(customerId, planId, options);
            return stripeSubscription;
        }

        public async Task<StripeSubscription> UpdateSubscriptionAsync(string customerId, string subscriptionId, StripePlan subscription)
        {
            StripeSubscription currentSubscription = await _stripe.SubscriptionService.GetAsync(subscriptionId);
            var updateOptions = new StripeSubscriptionUpdateOptions()
            {
                PlanId = subscription.Id
            };
            if (currentSubscription.Status == "trialing" && subscription.TrialPeriodDays > 0)
            {
                // if the current trialEnd is before the new subscription's trial WOULD end
                var newTrialEnd = DateTime.Now.AddDays(subscription.TrialPeriodDays.Value);
                if (newTrialEnd < currentSubscription.TrialEnd)
                    updateOptions.TrialEnd = newTrialEnd;
                else
                    updateOptions.TrialEnd = currentSubscription.TrialEnd;
            }
            else
            {
                updateOptions.EndTrialNow = true;
            }
            currentSubscription = await _stripe.SubscriptionService.UpdateAsync(subscriptionId, updateOptions);
            return currentSubscription;
        }

        public async Task<StripeSubscription> UpdateSubscriptionTax(string customerId, string subscriptionId, decimal taxPercent)
        {
            var updateOptions = new StripeSubscriptionUpdateOptions()
            {
                TaxPercent = taxPercent
            };
            StripeSubscription stripeSubscription = await _stripe.SubscriptionService.UpdateAsync(subscriptionId, updateOptions);
            return stripeSubscription;
        }

        public async Task<StripeSubscription> UserActiveSubscriptionAsync(string customerId)
        {
            IEnumerable<StripeSubscription> response = await UserActiveSubscriptionsAsync(customerId);
            return response.FirstOrDefault();
        }

        public async Task<IEnumerable<StripeSubscription>> UserActiveSubscriptionsAsync(string customerId)
        {
            IEnumerable<StripeSubscription> response = await UserSubscriptionsAsync(customerId);
            return response.Where(s => s.Status == "active" || s.Status == "trialing");
        }

        public async Task<IEnumerable<StripeSubscription>> UserSubscriptionsAsync(string customerId)
        {
            IEnumerable<StripeSubscription> response = await _stripe.SubscriptionService.ListAsync(new StripeSubscriptionListOptions()
            {
                CustomerId = customerId
            });
            return response;
        }
    }

}
