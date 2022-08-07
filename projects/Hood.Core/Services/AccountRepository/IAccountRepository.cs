using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hood.Services
{
    public interface IAuth0AccountRepository : IAccountRepository<Auth0User, Auth0Role>
    {        
        Task<bool> CreateAsync(Auth0User user);
    }
    public interface IPasswordAccountRepository : IAccountRepository<ApplicationUser, IdentityRole>
    {
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<IdentityResult> ChangePassword(ApplicationUser user, string oldPassword, string newPassword);
        Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string code, string password);
        Task SetEmailAsync(ApplicationUser modelToUpdate, string email);
        Task SetPhoneNumberAsync(ApplicationUser modelToUpdate, string phoneNumber);
    }
    public interface IAccountRepository<TUser, TRole> : IHoodAccountRepository
    {
        Task AddUserToRolesAsync(TUser user, TRole[] roles);
        Task<TRole> CreateRoleAsync(string role);
        Task DeleteRoleAsync(string role);
        Task DeleteUserAsync(string userId, ClaimsPrincipal adminUser);
        Task<MediaDirectory> GetDirectoryAsync(string id);
        Task<UserProfileView<TRole>> GetUserProfileViewById(string id);
        Task<TRole> GetRoleAsync(string role);
        Task<IPagedList<TRole>> GetRolesAsync(IPagedList<TRole> model);
        Task<IList<TRole>> GetRolesForUser(TUser user);
        Task<UserStatistics> GetStatisticsAsync();
        Task<TUser> GetUserByEmailAsync(string email, bool track = true);
        Task<TUser> GetUserByIdAsync(string id, bool track = true);
        Task<UserListModel> GetUserProfileViewsAsync(UserListModel model, IQueryable<UserProfileView<TRole>> query = null);
        Task RemoveUserFromRolesAsync(TUser user, TRole[] roles);
        Task SetupRolesAsync();
        Task UpdateProfileAsync(UserProfile user);
        Task UpdateUserAsync(TUser user);
    }
    public interface IHoodAccountRepository
    {
        Task<UserProfile> GetUserProfileByIdAsync(string id);
        Task<UserListModel> GetUserProfilesAsync(UserListModel model, IQueryable<IUserProfile> query = null);
        Task<IList<IUserProfile>> GetUsersInRole(string roleName);
        Task<bool> RoleExistsAsync(string role);

    }
}
