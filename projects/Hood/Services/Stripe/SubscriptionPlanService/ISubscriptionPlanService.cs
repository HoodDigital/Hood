using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Services
{
    /// <summary>
    /// Interface for loading and communicating with Stripe Subscription Plans
    /// </summary>
    public interface ISubscriptionPlanService
    {
        /// <summary>
        /// Finds the plan by identifier.
        /// </summary>
        /// <param name="planId">The stripe subscription plan identifier.</param>
        /// <returns></returns>
        Task<Stripe.Plan> FindByIdAsync(string planId);

        /// <summary>
        /// Returns all the subscription plans for this account.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Stripe.Plan>> GetAllAsync();

        /// <summary>
        /// This creates a new plan on the site.
        /// </summary>
        /// <param name="name">Name of the new plan.</param>
        /// <param name="amount">Total cost per interval of the plan. In Pence.</param>
        /// <param name="currency">Currency to charge in.</param>
        /// <param name="interval">NAme of the interval, "month", "year" etc.</param>
        /// <param name="intervalCount">Number of intervals to charge.</param>
        /// <param name="trialPeriodDays">Period before the user is charged.</param>
        /// <returns></returns>
        Task<Stripe.Plan> CreatePlan(string name, int amount, string colour, string description, string features, string currency = "gbp", string interval = "month", int intervalCount = 1, int trialPeriodDays = 30);

        /// <summary>
        /// Deletes the plan asynchronous.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        void DeletePlan(string planId);
    }
}
