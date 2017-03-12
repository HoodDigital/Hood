using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Hood.Infrastructure;
using Hood.Models;
using Stripe;
using System;

namespace Hood.Services
{
    public interface IAccountRepository
    {
        // Account stuff
        AccountInfo LoadAccountInfo(string userId);
        ApplicationUser GetCurrentUser(bool track = true);
        ApplicationUser GetUserById(string userId, bool track = true);
        OperationResult UpdateUser(ApplicationUser user);

        IList<IdentityRole> GetAllRoles();

        // Addresses
        OperationResult DeleteAddress(int id);
        Address GetAddressById(int id);
        OperationResult UpdateAddress(Address address);
        OperationResult SetBillingAddress(string userId, int id);
        OperationResult SetDeliveryAddress(string userId, int id);

        // Subscription Plans
        Task<OperationResult> AddSubscriptionPlan(Subscription subscription);
        Task<List<Subscription>> GetSubscriptionPlansAsync();
        Task<List<Subscription>> GetSubscriptionPlanLevels();
        Task<List<Subscription>> GetSubscriptionPlanAddons();
        Task<PagedList<Subscription>> GetPagedSubscriptions(ListFilters filters, string search, string sort);
        Task<Subscription> GetSubscriptionPlanById(int id);
        Task<Subscription> GetSubscriptionPlanByStripeId(string stripeId);
        Task<OperationResult> DeleteSubscriptionPlan(int id);
        Task<OperationResult> UpdateSubscription(Subscription model);

        // User Subscriptions
        Task<PagedList<ApplicationUser>> GetPagedSubscribers(ListFilters filters, string subcription);
        OperationResult SaveUserSubscription(UserSubscription newUserSub);
        OperationResult UpdateUserSubscription(UserSubscription newUserSub);
        Task CreateUserSubscription(int planId, string stripeToken, string cardId);
        Task UpgradeUserSubscription(int subscriptionId, int planId);
        Task CancelUserSubscription(int subscriptionId);
        Task RemoveUserSubscription(int subscriptionId);
        Task ReactivateUserSubscription(int subscriptionId);

        // Customer Objects
        void ResetBillingInfo();
        Task<ApplicationUser> GetUserByStripeId(string stripeId);
        Task<StripeCustomer> LoadCustomerObject(string stripeId, bool allowNullObject);
        Task ConfirmSubscriptionObject(StripeSubscription created, DateTime? eventTime);
        Task UpdateSubscriptionObject(StripeSubscription updated, DateTime? eventTime);
        Task RemoveUserSubscriptionObject(StripeSubscription updated, DateTime? eventTime);
        Task<UserSubscription> FindUserSubscriptionByStripeId(string id);
    }
}