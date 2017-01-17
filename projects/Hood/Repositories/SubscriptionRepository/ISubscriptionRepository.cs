using Hood.Infrastructure;
using Hood.Models;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface ISubscriptionRepository
    {
        // Subscriptions
        Task<OperationResult> Add(Subscription subscription);
        Task<List<Subscription>> GetAllAsync();
        Task<List<Subscription>> GetLevels();
        Task<List<Subscription>> GetAddons();
        Task<PagedList<Subscription>> GetPagedSubscriptions(ListFilters filters, string search, string sort);
        Task<Subscription> GetSubscriptionById(int id, bool nocache = false);
        Task<Subscription> GetSubscriptionByStripeId(string stripeId);
        Task<OperationResult> Delete(int id);
        Task<OperationResult> UpdateSubscription(Subscription model);

        // User Subscriptions
        /// <summary>
        /// Returns a paged list of all users in the subscription.
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="subscriptionId"></param>
        /// <param name="search"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        Task<PagedList<ApplicationUser>> GetPagedSubscribers(ListFilters filters, int subscriptionId, string search, string sort);
        /// <summary>
        /// This will create a user subsription using the token or card provided, once completed, add it to the database. Will throw an exception on error.
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="stripeToken"></param>
        /// <param name="cardId"></param>
        Task CreateUserSubscription(int planId, string stripeToken, string cardId);
        Task UpgradeUserSubscription(int subscriptionId, int planId);
        Task CancelUserSubscription(int subscriptionId);
        Task RemoveUserSubscription(int subscriptionId);
        Task ReactivateUserSubscription(int subscriptionId);

        Task<StripeCustomer> GetCustomerObject(string stripeId, bool allowNullObject);
        Task ConfirmSubscriptionObject(StripeSubscription created, DateTime? eventTime);
        Task UpdateSubscriptionObject(StripeSubscription updated, DateTime? eventTime);
        Task RemoveUserSubscriptionObject(StripeSubscription updated, DateTime? eventTime);
        Task<UserSubscription> FindUserSubscriptionByStripeId(string id);
    }
}