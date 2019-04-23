using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;

namespace Hood.Services
{
    /// <summary>
    /// Interface for loading and communicating with Stripe Subscriptions
    /// </summary>
    public interface ISubscriptionService
    {

        /// <summary>
        /// Finds the by identifier.
        /// </summary>
        /// <param name="stripeSubscriptionId">The stripe subscription identifier.</param>
        /// <returns></returns>
        Task<Stripe.Subscription> FindById(string customerId, string subscriptionId);

        /// <summary>
        /// Subscribes the user asynchronous.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="trialPeriodInDays">The trial period in days.</param>
        /// <param name="taxPercent">The tax percent.</param>
        /// <param name="stripeId">The stripe identifier.</param>
        /// <returns>
        /// The subscription
        /// </returns>
        Task<Stripe.Subscription> SubscribeUserAsync(string customerId, string planId, DateTime? trialEnd = null, decimal taxPercent = 0);

        /// <summary>
        /// Gets the User's subscriptions asynchronous.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        Task<IEnumerable<Stripe.Subscription>> UserSubscriptionsAsync(string customerId);

        /// <summary>
        /// Get the User's active subscription asynchronous. Only the first (valid if your customers can have only 1 subscription at a time).
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The subscription</returns>
        Task<Stripe.Subscription> UserActiveSubscriptionAsync(string customerId);

        /// <summary>
        /// Get the User's active subscriptions asynchronous.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The list of Subscriptions</returns>
        Task<IEnumerable<Stripe.Subscription>> UserActiveSubscriptionsAsync(string customerId);

        /// <summary>
        /// Ends the subscription asynchronous.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="subscriptionEnDateTime">The subscription en date time.</param>
        /// <param name="reasonToCancel">The reason to cancel.</param>
        /// <returns></returns>
        Task<Stripe.Subscription> CancelSubscriptionAsync(string customerId, string subscriptionId, bool cancelAtPeriodEnd = true);

        /// <summary>
        /// Changes the plan of the subscription, asyncronous
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns></returns>
        Task<Stripe.Subscription> UpdateSubscriptionAsync(string customerId, string subscriptionId, Stripe.Plan subscription);

        /// <summary>
        /// Updates the subscription tax.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="taxPercent">The tax percent.</param>
        /// <returns></returns>
        Task<Stripe.Subscription> UpdateSubscriptionTax(string customerId, string subscriptionId, decimal taxPercent);

    }
}