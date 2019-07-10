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
        Task UpdateUserAsync(ApplicationUser user);
        Task DeleteUserAsync(ApplicationUser user);
        Task<List<UserAccessCode>> GetAccessCodesAsync(string id);
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

        #region Subscription Plans
        Task<SubscriptionSearchModel> GetSubscriptionPlansAsync(SubscriptionSearchModel model = null);
        Task<Subscription> GetSubscriptionPlanByIdAsync(int id);
        Task<Subscription> GetSubscriptionPlanByStripeIdAsync(string stripeId);
        Task<Subscription> AddSubscriptionPlanAsync(Models.Subscription subscription);
        Task UpdateSubscriptionAsync(Subscription model);
        Task DeleteSubscriptionPlanAsync(int id);
        #endregion

        #region User Subscriptions
        Task<UserSubscriptionListModel> GetUserSubscriptionsAsync(UserSubscriptionListModel model);
        Task<UserSubscription> CreateUserSubscriptionAsync(int planId, string stripeToken, string cardId);
        Task<UserSubscription> UpdateUserSubscriptionAsync(UserSubscription userSubscription);
        Task<UserSubscription> UpgradeUserSubscriptionAsync(int subscriptionId, int planId);
        Task<UserSubscription> CancelUserSubscriptionAsync(int subscriptionId);
        Task<UserSubscription> ReactivateUserSubscriptionAsync(int subscriptionId);
        Task<UserSubscription> RemoveUserSubscriptionAsync(int subscriptionId);
        #endregion

        #region WebHooks
        Task ConfirmSubscriptionObjectAsync(Stripe.Subscription created, DateTime? eventTime);
        Task UpdateSubscriptionObjectAsync(Stripe.Subscription updated, DateTime? eventTime);
        Task RemoveUserSubscriptionObjectAsync(Stripe.Subscription updated, DateTime? eventTime);
        Task<UserSubscription> GetUserSubscriptionByStripeIdAsync(string id);

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