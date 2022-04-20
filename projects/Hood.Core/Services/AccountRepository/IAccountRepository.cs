using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IAccountRepository
    {
        #region Account stuff
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<ApplicationUser> GetUserByIdAsync(string id, bool track = true);
        Task<ApplicationUser> GetUserByEmailAsync(string email, bool track = true);
        Task<UserProfile> GetUserProfileByIdAsync(string id);
        Task UpdateUserAsync(ApplicationUser user);
        Task DeleteUserAsync(string userId, System.Security.Claims.ClaimsPrincipal adminUser);
        Task<MediaDirectory> GetDirectoryAsync(string id);
        Task SetEmailAsync(ApplicationUser modelToUpdate, string email);
        Task SetPhoneNumberAsync(ApplicationUser modelToUpdate, string email);
        Task<IdentityResult> ChangePassword(ApplicationUser user, string oldPassword, string newPassword);
        Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string code);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string code, string password);
        #endregion

        #region Profiles
        Task<UserListModel> GetUserProfilesAsync(UserListModel model, IQueryable<UserProfile> query = null);
        Task<UserProfile> GetProfileAsync(string id);
        Task UpdateProfileAsync(UserProfile user);
        #endregion

        #region Roles
        bool SupportsRoles();
        Task<IPagedList<ApplicationRole>> GetRolesAsync(IPagedList<ApplicationRole> model = null);
        Task<IList<ApplicationRole>> GetRolesForUser(ApplicationUser user);
        Task<IList<ApplicationUser>> GetUsersInRole(string role);
        Task<bool> RoleExistsAsync(string role);
        Task<ApplicationRole> GetRoleAsync(string role);
        Task<ApplicationRole> CreateRoleAsync(string role);
        Task DeleteRoleAsync(string role);
        Task AddUserToRolesAsync(ApplicationUser user, ApplicationRole[] roles);
        Task RemoveUserFromRolesAsync(ApplicationUser user, ApplicationRole[] roles);
        #endregion

        #region Addresses
        Task<Models.Address> GetAddressByIdAsync(int id);
        Task DeleteAddressAsync(int id);
        Task UpdateAddressAsync(Models.Address address);
        Task SetBillingAddressAsync(string userId, int id);
        Task SetDeliveryAddressAsync(string userId, int id);
        #endregion

        #region Statistics
        Task<UserStatistics> GetStatisticsAsync();
        #endregion
    }
}