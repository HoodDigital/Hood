using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;

namespace Hood.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private IStripeService _stripe;
        public SubscriptionPlanService(IStripeService stripe)
        {
            _stripe = stripe;
        }

        public async Task<StripePlan> CreatePlan(string name, int amount, string colour, string description, string features, string currency = "gbp", string interval = "month", int intervalCount = 1, int trialPeriodDays = 30)
        {
            var myPlan = new StripePlanCreateOptions();
            myPlan.Id = Guid.NewGuid().ToString();
            myPlan.Amount = amount;                     // all amounts on Stripe are in cents, pence, etc
            myPlan.Currency = currency;                 // "usd" only supported right now
            myPlan.Interval = interval;                 // "month" or "year"
            myPlan.IntervalCount = intervalCount;       // optional
            myPlan.Name = name;
            myPlan.TrialPeriodDays = trialPeriodDays;   // amount of time that will lapse before the customer is billed
            myPlan.Metadata.Add("Colour", colour);
            myPlan.Metadata.Add("Description", description);
            myPlan.Metadata.Add("Features", features);
            StripePlan response = await _stripe.PlanService.CreateAsync(myPlan);
            return response;
        }
        public void DeletePlan(string planId)
        {
            _stripe.PlanService.DeleteAsync(planId);
        }
        public async Task<StripePlan> FindByIdAsync(string planId)
        {
            StripePlan response = await _stripe.PlanService.GetAsync(planId);
            return response;
        }
        public async Task<IEnumerable<StripePlan>> GetAllAsync()
        {
            var stripeSubs = await _stripe.PlanService.ListAsync();
            return stripeSubs;
        }
    }
}
