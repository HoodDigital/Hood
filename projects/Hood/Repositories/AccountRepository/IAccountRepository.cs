using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Identity;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IAccountRepository
    {
        #region Account stuff
        Task<ApplicationUser> GetCurrentUserAsync(bool track = true);
        Task<ApplicationUser> GetUserByIdAsync(string id, bool track = true);
        Task<ApplicationUser> GetUserByEmailAsync(string email, bool track = true);
        Task<ApplicationUser> GetUserByStripeIdAsync(string stripeId, bool track = true);
        Task<UserProfile> GetUserProfileByIdAsync(string id);
        Task UpdateUserAsync(ApplicationUser user);
        Task DeleteUserAsync(string userId, System.Security.Claims.ClaimsPrincipal adminUser);
        Task<List<UserAccessCode>> GetAccessCodesAsync(string id);
        Task<MediaDirectory> GetDirectoryAsync(string id);
        #endregion

        #region Profiles
        Task<UserListModel> GetUserProfilesAsync(UserListModel model);
        Task<UserProfile> GetProfileAsync(string id);
        Task UpdateProfileAsync(UserProfile user);
        #endregion        
        
        #region Roles
        Task<IList<IdentityRole>> GetAllRolesAsync();
        #endregion

        #region Addresses
        Task DeleteAddressAsync(int id);
        Task<Models.Address> GetAddressByIdAsync(int id);
        Task UpdateAddressAsync(Models.Address address);
        Task SetBillingAddressAsync(string userId, int id);
        Task SetDeliveryAddressAsync(string userId, int id);
        #endregion

        #region Stripe customer object
        Task<Stripe.Customer> GetOrCreateStripeCustomerForUser(string userId);
        Task<List<Stripe.Customer>> GetMatchingCustomerObjectsAsync(string email);
        Task<ApplicationUser> CreateLocalUserForCustomerObject(Customer customer);
        #endregion

        #region Subscription Products
        Task<SubscriptionProductListModel> GetSubscriptionProductsAsync(SubscriptionProductListModel model = null);
        Task<StripeProductListModel> GetStripeProductsAsync(StripeProductListModel model);
        Task<SubscriptionProduct> GetSubscriptionProductByIdAsync(int id);
        Task<SubscriptionProduct> CreateSubscriptionProductAsync(string name, string stripeId);
        Task<SubscriptionProduct> UpdateSubscriptionProductAsync(SubscriptionProduct model);
        Task<SubscriptionProduct> DeleteSubscriptionProductAsync(int id);
        Task<SubscriptionProduct> SyncSubscriptionProductAsync(int? id, string stripeId);
        #endregion

        #region Subscription Plans
        Task<SubscriptionPlanListModel> GetSubscriptionPlansAsync(SubscriptionPlanListModel model = null);
        Task<StripePlanListModel> GetStripeSubscriptionPlansAsync(StripePlanListModel model);
        Task<Models.Subscription> GetSubscriptionPlanByIdAsync(int id);
        Task<Models.Subscription> GetSubscriptionPlanByStripeIdAsync(string stripeId);
        Task<Models.Subscription> CreateSubscriptionPlanAsync(Models.Subscription subscription);
        Task<Models.Subscription> UpdateSubscriptionPlanAsync(Models.Subscription model);
        Task<Models.Subscription> DeleteSubscriptionPlanAsync(int id);
        Task<Models.Subscription> SyncSubscriptionPlanAsync(int? id, string stripeId);
        #endregion

        #region User Subscriptions
        Task<UserSubscriptionListModel> GetUserSubscriptionsAsync(UserSubscriptionListModel model);
        Task<UserSubscription> GetUserSubscriptionByIdAsync(int id);
        Task<UserSubscription> GetUserSubscriptionByStripeIdAsync(string stripeId);
        Task<UserSubscription> CreateUserSubscription(int planId, string userId, Stripe.Subscription newSubscription);
        Task<UserSubscription> DeleteUserSubscriptionAsync(int id);
        Task<UserSubscription> CancelUserSubscriptionAsync(int subscriptionId, bool cancelAtPeriodEnd = true, bool invoiceNow = false, bool prorate = false);
        Task<UserSubscription> ReactivateUserSubscriptionAsync(int subscriptionId);
        Task<UserSubscription> SyncUserSubscriptionAsync(int? id, string stripeId);
        Task<UserSubscription> SwitchUserSubscriptionAsync(int subscriptionId, int newPlanId);
        #endregion

        #region Statistics
        Task<object> GetStatisticsAsync();
        Task<object> GetSubscriptionStatisticsAsync();
        #endregion
    }
}