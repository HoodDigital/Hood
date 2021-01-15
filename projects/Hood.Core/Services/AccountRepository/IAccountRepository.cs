using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Identity;
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

        #region Statistics
        Task<object> GetStatisticsAsync();
        #endregion
    }
}