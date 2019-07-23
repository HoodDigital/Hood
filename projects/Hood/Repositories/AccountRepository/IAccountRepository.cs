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
        #region Account stuff
        Task<ApplicationUser> GetCurrentUserAsync(bool track = true);
        Task<ApplicationUser> GetUserByIdAsync(string id, bool track = true);
        Task<ApplicationUser> GetUserByEmailAsync(string email, bool track = true);
        Task<ApplicationUser> GetUserByStripeIdAsync(string stripeId, bool track = true);
        Task<UserProfile> GetUserProfileByIdAsync(string id);
        Task UpdateUserAsync(ApplicationUser user);
        Task DeleteUserAsync(string userId, System.Security.Claims.ClaimsPrincipal adminUser);
        Task<List<UserAccessCode>> GetAccessCodesAsync(string id);
        Task<MediaDirectory> GetDirectoryAsync(string userId);
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
        Task<Address> GetAddressByIdAsync(int id);
        Task UpdateAddressAsync(Models.Address address);
        Task SetBillingAddressAsync(string userId, int id);
        Task SetDeliveryAddressAsync(string userId, int id);
        #endregion

        #region Stripe customer object
        Task<Stripe.Customer> GetCustomerObjectAsync(string stripeId);
        Task<List<Stripe.Customer>> GetMatchingCustomerObjectsAsync(string email);
        #endregion

        #region Subscription Products
        Task<SubscriptionProductListModel> GetSubscriptionProductsAsync(SubscriptionProductListModel model = null);
        Task<StripeProductListModel> GetStripeProductsAsync(StripeProductListModel model);
        Task<SubscriptionProduct> GetSubscriptionProductByIdAsync(int id);
        Task<SubscriptionProduct> CreateSubscriptionProductAsync(string name, string stripeId);
        Task<SubscriptionProduct> UpdateSubscriptionProductAsync(SubscriptionProduct model);
        Task DeleteSubscriptionProductAsync(int id);
        Task<SubscriptionProduct> SyncSubscriptionProductAsync(int? id, string stripeId);
        #endregion

        #region Subscription Plans
        Task<SubscriptionPlanListModel> GetSubscriptionPlansAsync(SubscriptionPlanListModel model = null);
        Task<StripePlanListModel> GetStripeSubscriptionPlansAsync(StripePlanListModel model);
        Task<Subscription> GetSubscriptionPlanByIdAsync(int id);
        Task<Subscription> GetSubscriptionPlanByStripeIdAsync(string stripeId);
        Task<Subscription> CreateSubscriptionPlanAsync(Models.Subscription subscription);
        Task<Subscription> UpdateSubscriptionPlanAsync(Subscription model);
        Task DeleteSubscriptionPlanAsync(int id);
        Task<Models.Subscription> SyncSubscriptionPlanAsync(int? id, string stripeId);
        #endregion

        #region User Subscriptions
        Task<UserSubscriptionListModel> GetUserSubscriptionsAsync(UserSubscriptionListModel model);
        Task<UserSubscription> GetUserSubscriptionByIdAsync(int id);
        Task<UserSubscription> GetUserSubscriptionByStripeIdAsync(string stripeId);
        Task<UserSubscription> CreateUserSubscriptionAsync(int planId, string stripeToken, string cardId);
        Task<UserSubscription> UpdateUserSubscriptionAsync(UserSubscription userSubscription);
        Task<UserSubscription> UpgradeUserSubscriptionAsync(int subscriptionId, int planId);
        Task<UserSubscription> CancelUserSubscriptionAsync(int subscriptionId);
        Task<UserSubscription> ReactivateUserSubscriptionAsync(int subscriptionId);
        Task<UserSubscription> RemoveUserSubscriptionAsync(int subscriptionId);
        Task<UserSubscription> SyncUserSubscriptionAsync(int id);
        #endregion

        #region WebHooks
        Task ConfirmSubscriptionObjectAsync(Stripe.Subscription created, DateTime? eventTime);
        Task UpdateSubscriptionObjectAsync(Stripe.Subscription updated, DateTime? eventTime);
        Task RemoveUserSubscriptionObjectAsync(Stripe.Subscription updated, DateTime? eventTime);

        #endregion

        #region Statistics
        Task<object> GetStatisticsAsync();
        Task<object> GetSubscriptionStatisticsAsync();
        #endregion

        #region Obsolete
        [Obsolete("Use _userManager.GetUserSubscriptionView(ClaimsPrincipal principal) from now on.", true)]
        AccountInfo LoadAccountInfo(string userId);
        #endregion
    }
}