
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
        private readonly IStripeService _stripe;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionService(IStripeService stripe,
                                   UserManager<ApplicationUser> userManager)
        {
            _stripe = stripe;
            _userManager = userManager;
        }

        public async Task<Stripe.Subscription> CancelSubscriptionAsync(string customerId, string subscriptionId, bool cancelAtPeriodEnd = true)
        {
            if (!cancelAtPeriodEnd)
                return await _stripe.SubscriptionService.CancelAsync(subscriptionId, new SubscriptionCancelOptions() { });  

            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false,
            };
            return await _stripe.SubscriptionService.UpdateAsync(subscriptionId, options);

        }

        public async Task<Stripe.Subscription> FindById(string customerId, string subscriptionId)
        {
            return await _stripe.SubscriptionService.GetAsync(subscriptionId);
        }

        public async Task<Stripe.Subscription> SubscribeUserAsync(string customerId, string planId, DateTime? trialEnd = null)
        {
            var options = new Stripe.SubscriptionCreateOptions()
            {
                TrialEnd = trialEnd,
                CustomerId = customerId,
                Items = new List<SubscriptionItemOption>()
                {
                    new SubscriptionItemOption()
                    {
                        PlanId = planId,
                        Quantity = 1
                    }
                }
            };
            Stripe.Subscription stripeSubscription = await _stripe.SubscriptionService.CreateAsync(options);
            return stripeSubscription;
        }

        public async Task<Stripe.Subscription> UpdateSubscriptionAsync(string customerId, string subscriptionId, Stripe.Plan subscription)
        {
            Stripe.Subscription currentSubscription = await _stripe.SubscriptionService.GetAsync(subscriptionId);
            var updateOptions = new Stripe.SubscriptionUpdateOptions()
            {
                Items = new List<Stripe.SubscriptionItemUpdateOption>()
                {
                    new Stripe.SubscriptionItemUpdateOption()
                    {
                        Id = subscriptionId
                    }
                }
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


        public async Task<Stripe.Subscription> UserActiveSubscriptionAsync(string customerId)
        {
            IEnumerable<Stripe.Subscription> response = await UserActiveSubscriptionsAsync(customerId);
            return response.FirstOrDefault();
        }

        public async Task<IEnumerable<Stripe.Subscription>> UserActiveSubscriptionsAsync(string customerId)
        {
            IEnumerable<Stripe.Subscription> response = await UserSubscriptionsAsync(customerId);
            return response.Where(s => s.Status == "active" || s.Status == "trialing");
        }

        public async Task<IEnumerable<Stripe.Subscription>> UserSubscriptionsAsync(string customerId)
        {
            IEnumerable<Stripe.Subscription> response = await _stripe.SubscriptionService.ListAsync(new Stripe.SubscriptionListOptions()
            {
                CustomerId = customerId
            });
            return response;
        }
    }

}
