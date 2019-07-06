using Hood.Infrastructure;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IAccountRepository
    {
        // Account stuff
        [Obsolete("Use _userManager.GetUserSubscriptionView(ClaimsPrincipal principal) from now on.", true)]
        AccountInfo LoadAccountInfo(string userId);
        ApplicationUser GetCurrentUser(bool track = true);
        ApplicationUser GetUserById(string userId, bool track = true);
        ApplicationUser GetUserByEmail(string email, bool track = true);
        OperationResult UpdateUser(ApplicationUser user);
        Task DeleteUserAsync(ApplicationUser user);

        IList<IdentityRole> GetAllRoles();

        // Addresses
        OperationResult DeleteAddress(int id);
        Models.Address GetAddressById(int id);
        OperationResult UpdateAddress(Models.Address address);
        OperationResult SetBillingAddress(string userId, int id);
        OperationResult SetDeliveryAddress(string userId, int id);

        // Subscription Plans
        Task<Models.Subscription> AddSubscriptionPlan(Models.Subscription subscription);
        Task<List<Models.Subscription>> GetSubscriptionPlansAsync();
        Task<List<Models.Subscription>> GetSubscriptionPlanLevels(string category = null);
        Task<List<Models.Subscription>> GetSubscriptionPlanAddons();
        Task<SubscriptionSearchModel> GetPagedSubscriptionPlans(SubscriptionSearchModel model);
        Task<Models.Subscription> GetSubscriptionPlanById(int id);
        Task<Models.Subscription> GetSubscriptionPlanByStripeId(string stripeId);
        Task DeleteSubscriptionPlan(int id);
        Task UpdateSubscription(Models.Subscription model);

        // User Subscriptions
        Task<SubscriberSearchModel> GetPagedSubscribers(SubscriberSearchModel model);
        Task<UserSubscription> UpdateUserSubscription(UserSubscription newUserSub);
        Task<UserSubscription> CreateUserSubscription(int planId, string stripeToken, string cardId);
        Task<UserSubscription> UpgradeUserSubscription(int subscriptionId, int planId);
        Task<UserSubscription> CancelUserSubscription(int subscriptionId);
        Task<UserSubscription> RemoveUserSubscription(int subscriptionId);
        Task<UserSubscription> ReactivateUserSubscription(int subscriptionId);

        // Customer Objects
        void ResetBillingInfo();
        Task<ApplicationUser> GetUserByStripeId(string stripeId);
        Task<Stripe.Customer> LoadCustomerObject(string stripeId, bool allowNullObject);
        string ConfirmSubscriptionObject(Stripe.Subscription created, DateTime? eventTime);
        string UpdateSubscriptionObject(Stripe.Subscription updated, DateTime? eventTime);
        string RemoveUserSubscriptionObject(Stripe.Subscription updated, DateTime? eventTime);
        UserSubscription FindUserSubscriptionByStripeId(string id);

        // Stats
        object GetStatistics();
        object GetSubscriptionStatistics();
    }
}