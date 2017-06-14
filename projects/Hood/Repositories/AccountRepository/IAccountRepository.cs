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
        ApplicationUser GetUserByEmail(string email, bool track = true);
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
        Task<UserSubscription> SaveUserSubscription(UserSubscription newUserSub);
        Task<UserSubscription> UpdateUserSubscription(UserSubscription newUserSub);
        Task<UserSubscription> CreateUserSubscription(int planId, string stripeToken, string cardId);
        Task<UserSubscription> UpgradeUserSubscription(int subscriptionId, int planId);
        Task<UserSubscription> CancelUserSubscription(int subscriptionId);
        Task<UserSubscription> RemoveUserSubscription(int subscriptionId);
        Task<UserSubscription> ReactivateUserSubscription(int subscriptionId);

        // Customer Objects
        void ResetBillingInfo();
        Task<ApplicationUser> GetUserByStripeId(string stripeId);
        Task<StripeCustomer> LoadCustomerObject(string stripeId, bool allowNullObject);
        string ConfirmSubscriptionObject(StripeSubscription created, DateTime? eventTime);
        string UpdateSubscriptionObject(StripeSubscription updated, DateTime? eventTime);
        string RemoveUserSubscriptionObject(StripeSubscription updated, DateTime? eventTime);
        UserSubscription FindUserSubscriptionByStripeId(string id);
    }
}