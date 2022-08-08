using Hood.Contexts;
using Hood.Core;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Hood.Services
{

    public class AccountRepository : IPasswordAccountRepository
    {
        protected readonly IdentityContext _db;
        protected readonly HoodDbContext _hoodDb;
        private IQueryable<ApplicationUser> Users { get { return _db.Users.Include(u => u.UserProfile); } }
        private IQueryable<IdentityRole> Roles { get { return _db.Roles; } }
        private IQueryable<IdentityUserRole<string>> UserRoles { get { return _db.UserRoles; } }
        private UserManager<ApplicationUser> UserManager => Engine.Services.Resolve<UserManager<ApplicationUser>>();
        private RoleManager<IdentityRole> RoleManager => Engine.Services.Resolve<RoleManager<IdentityRole>>();
        
        public AccountRepository()
        {
            _db = Engine.Services.Resolve<IdentityContext>();
            _hoodDb = Engine.Services.Resolve<HoodDbContext>();
        }

        #region Account stuff
        public virtual async Task<ApplicationUser> GetUserByIdAsync(string id, bool track = true)
        {
            if (!id.IsSet())
            {
                return null;
            }

            IQueryable<ApplicationUser> query = Users;
            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.SingleOrDefaultAsync(u => u.Id == id);
        }
        public virtual async Task<ApplicationUser> GetUserByEmailAsync(string email, bool track = true)
        {
            if (!email.IsSet())
            {
                return null;
            }

            IQueryable<ApplicationUser> query = Users;
            if (!track)
            {
                query = query.AsNoTracking();
            }

            return await query.SingleOrDefaultAsync(u => u.Email == email);
        }
        public virtual async Task UpdateUserAsync(ApplicationUser user)
        {
            _db.Update(user);
            await _db.SaveChangesAsync();
        }
        public virtual async Task DeleteUserAsync(string userId, System.Security.Claims.ClaimsPrincipal adminUser)
        {
            var user = await PrepareUserForDelete(userId, adminUser);

            IList<UserLoginInfo> logins = await UserManager.GetLoginsAsync(user);
            foreach (UserLoginInfo li in logins)
            {
                await UserManager.RemoveLoginAsync(user, li.LoginProvider, li.ProviderKey);
            }

            IList<string> roles = await UserManager.GetRolesAsync(user);
            foreach (string role in roles)
            {
                await UserManager.RemoveFromRoleAsync(user, role);
            }

            IList<System.Security.Claims.Claim> claims = await UserManager.GetClaimsAsync(user);
            foreach (System.Security.Claims.Claim claim in claims)
            {
                await UserManager.RemoveClaimAsync(user, claim);
            }

            await UserManager.DeleteAsync(user);
        }
        protected virtual async Task<ApplicationUser> PrepareUserForDelete(string userId, System.Security.Claims.ClaimsPrincipal adminUser)
        {
            ApplicationUser user = await Users
                .SingleOrDefaultAsync(u => u.Id == userId);

            if (user.Email == Engine.Configuration.SuperAdminEmail)
            {
                throw new Exception("You cannot delete the site owner account, the owner is set via an environment variable and cannot be changed from the admin area.");
            }

            ApplicationUser siteOwner = await Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == Engine.Configuration.SuperAdminEmail);
            if (siteOwner == null)
            {
                throw new Exception("Could not load the owner account, check your settings, the owner is set via an environment variable and cannot be changed from the admin area.");
            }

            if (!adminUser.IsAdminOrBetter() && adminUser.GetLocalUserId() != user.Id)
            {
                throw new Exception("You do not have permission to delete this user.");
            }

            await _db.SaveChangesAsync();

            return user;
        }
        public virtual async Task<MediaDirectory> GetDirectoryAsync(string id)
        {
            MediaDirectory directory = await _hoodDb.MediaDirectories.SingleOrDefaultAsync(md => md.OwnerId == id && md.Type == DirectoryType.User);
            if (directory == null)
            {
                MediaDirectory userDirectory = await _hoodDb.MediaDirectories.SingleOrDefaultAsync(md => md.Slug == MediaManager.UserDirectorySlug && md.Type == DirectoryType.System);
                ApplicationUser user = await GetUserByIdAsync(id);
                if (user == null)
                {
                    throw new Exception("No user found to add/get directory for.");
                }

                directory = new MediaDirectory()
                {
                    OwnerId = id,
                    Type = DirectoryType.User,
                    ParentId = userDirectory.Id,
                    DisplayName = user.UserName,
                    Slug = user.Id
                };
                _db.Add(directory);
                await _db.SaveChangesAsync();
            }
            return directory;
        }
        public virtual async Task SetEmailAsync(ApplicationUser modelToUpdate, string email)
        {
            IdentityResult setEmailResult = await UserManager.SetEmailAsync(modelToUpdate, email);
            if (!setEmailResult.Succeeded)
            {
                throw new Exception(setEmailResult.Errors.FirstOrDefault().Description);
            }
        }
        public virtual async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string code)
        {
            return await UserManager.ConfirmEmailAsync(user, code);
        }
        public virtual async Task SetPhoneNumberAsync(ApplicationUser modelToUpdate, string phoneNumber)
        {
            IdentityResult setPhoneResult = await UserManager.SetPhoneNumberAsync(modelToUpdate, phoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                throw new Exception(setPhoneResult.Errors.FirstOrDefault().Description);
            }
        }
        public virtual async Task<IdentityResult> ChangePassword(ApplicationUser user, string oldPassword, string newPassword)
        {
            return await UserManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }
        public virtual async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string code, string password)
        {
            return await UserManager.ResetPasswordAsync(user, code, password);
        }
        public virtual async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            return await UserManager.CreateAsync(user, password);
        }
        #endregion

        #region Profiles         
        public virtual async Task<UserProfile> GetUserProfileByIdAsync(string id)
        {
            if (!id.IsSet())
            {
                return null;
            }
            return await _db.UserProfiles.SingleOrDefaultAsync(u => u.Id == id);
        }
        public virtual async Task<UserListModel> GetUserProfilesAsync(UserListModel model, IQueryable<IUserProfile> query = null)
        {
            if (query == null)
            {
                query = _db.UserProfiles.AsQueryable();
            }

            if (!string.IsNullOrEmpty(model.Search))
            {
                query = query.Where(u =>
                    u.UserName.Contains(model.Search) ||
                    u.Email.Contains(model.Search) ||
                    u.DisplayName.Contains(model.Search) ||
                    u.FirstName.Contains(model.Search) ||
                    u.LastName.Contains(model.Search)
                );
            }

            switch (model.Order)
            {
                case "UserName":
                    query = query.OrderBy(n => n.UserName);
                    break;
                case "Email":
                    query = query.OrderBy(n => n.Email);
                    break;
                case "LastName":
                    query = query.OrderBy(n => n.LastName);
                    break;

                case "UserNameDesc":
                    query = query.OrderByDescending(n => n.UserName);
                    break;
                case "EmailDesc":
                    query = query.OrderByDescending(n => n.Email);
                    break;
                case "LastNameDesc":
                    query = query.OrderByDescending(n => n.LastName);
                    break;

                default:
                    query = query.OrderBy(n => n.UserName);
                    break;
            }

            await model.ReloadAsync(query);

            return model;
        }
        public virtual async Task<UserListModel<UserProfileView<IdentityRole>>> GetUserProfileViewsAsync(UserListModel<UserProfileView<IdentityRole>> model)
        {
            var query = _db.UserProfileViews.AsQueryable();    

            if (model.Role.IsSet())
            {
                query = query.Where(q => q.RoleIds.Contains(model.Role));
            }

            if (model.RoleIds != null && model.RoleIds.Count > 0)
            {
                query = query.Where(q => q.RoleIds != null && model.RoleIds.ToArray().Any(m => q.RoleIds.Contains(m)));
            }

            if (!string.IsNullOrEmpty(model.Search))
            {
                query = query.Where(u =>
                    u.UserName.Contains(model.Search) ||
                    u.Email.Contains(model.Search) ||
                    u.DisplayName.Contains(model.Search) ||
                    u.FirstName.Contains(model.Search) ||
                    u.LastName.Contains(model.Search)
                );
            }

            if (model.Active)
            {
                query = query.Where(q => q.Active);
            }

            if (model.Inactive)
            {
                query = query.Where(q => !q.Active);
            }

            if (model.PhoneUnconfirmed)
            {
                query = query.Where(q => !q.PhoneNumberConfirmed);
            }

            if (model.EmailUnconfirmed)
            {
                query = query.Where(q => !q.EmailConfirmed);
            }

            if (model.Unused)
            {
                query = query.Where(q => q.LastLoginLocation == null || q.LastLoginLocation == null || q.LastLogOn == DateTime.MinValue);
            }

            switch (model.Order)
            {
                case "UserName":
                    query = query.OrderBy(n => n.UserName);
                    break;
                case "Email":
                    query = query.OrderBy(n => n.Email);
                    break;
                case "LastName":
                    query = query.OrderBy(n => n.LastName);
                    break;
                case "LastLogOn":
                    query = query.OrderByDescending(n => n.LastLogOn);
                    break;

                case "UserNameDesc":
                    query = query.OrderByDescending(n => n.UserName);
                    break;
                case "EmailDesc":
                    query = query.OrderByDescending(n => n.Email);
                    break;
                case "LastNameDesc":
                    query = query.OrderByDescending(n => n.LastName);
                    break;

                default:
                    query = query.OrderBy(n => n.UserName);
                    break;
            }

            await model.ReloadAsync(query);

            return model;
        }
        public virtual async Task<UserProfileView<IdentityRole>> GetUserProfileViewById(string id)
        {
            UserProfileView<IdentityRole> profile = await _db.UserProfileViews.FirstOrDefaultAsync(u => u.Id == id);
            return profile;
        }
        public virtual async Task<ApplicationUser> UpdateProfileAsync(ApplicationUser user, IUserProfile profile)
        {
            foreach (PropertyInfo property in typeof(IUserProfile).GetProperties())
            {
                property.SetValue(user.UserProfile, property.GetValue(profile));
            }

            foreach (PropertyInfo property in typeof(IName).GetProperties())
            {
                property.SetValue(user.UserProfile, property.GetValue(profile));
            }

            foreach (PropertyInfo property in typeof(IJsonMetadata).GetProperties())
            {
                property.SetValue(user.UserProfile, property.GetValue(profile));
            }

            _db.Update(user);
            await _db.SaveChangesAsync();
            
            return user;
        }
        #endregion

        #region Roles
        public virtual async Task<IPagedList<IdentityRole>> GetRolesAsync(IPagedList<IdentityRole> model)
        {
            if (model == null)
            {
                model = new PagedList<IdentityRole>() { PageIndex = 0, PageSize = 50 };
            }
            var query = _db.Roles.AsQueryable();

            if (!string.IsNullOrEmpty(model.Search))
            {
                query = query.Where(u => u.Name.Contains(model.Search));
            }

            switch (model.Order)
            {
                case "Name":
                    query = query.OrderBy(n => n.Name);
                    break;
                case "NameDesc":
                    query = query.OrderByDescending(n => n.Name);
                    break;
                default:
                    query = query.OrderBy(n => n.Name);
                    break;
            }

            await model.ReloadAsync(query);

            return model;
        }
        public virtual async Task<IList<IdentityRole>> GetRolesForUser(ApplicationUser user)
        {
            var userId = user.Id;
            var query = from userRole in UserRoles
                        join role in Roles on userRole.RoleId equals role.Id
                        where userRole.UserId.Equals(userId)
                        select role;
            return await query.ToListAsync();
        }
        public virtual async Task<IList<IUserProfile>> GetUsersInRole(string roleName)
        {
            var role = await GetRoleAsync(roleName);
            if (role != null)
            {
                var query = from userrole in UserRoles
                            join user in Users on userrole.UserId equals user.Id
                            where userrole.RoleId.Equals(role.Id)
                            select user.UserProfile;

                return await query.ToListAsync<IUserProfile>();
            }
            return new List<IUserProfile>();
        }
        public virtual async Task<bool> RoleExistsAsync(string role)
        {
            return await GetRoleAsync(role) != null;
        }
        public virtual async Task<IdentityRole> GetRoleAsync(string role)
        {
            return await _db.Roles.SingleOrDefaultAsync(r => r.NormalizedName == role.ToUpperInvariant());
        }
        public virtual async Task<IdentityRole> CreateRoleAsync(string role)
        {
            var roleObject = await GetRoleAsync(role);
            if (roleObject == null)
            {
                roleObject = new IdentityRole()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = role,
                    NormalizedName = role.ToUpperInvariant(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                _db.Roles.Add(roleObject);
                await _db.SaveChangesAsync();
            }
            return roleObject;
        }
        public virtual async Task DeleteRoleAsync(string role)
        {
            var identityRole = await GetRoleAsync(role);
            _db.Roles.Remove(identityRole);
            await _db.SaveChangesAsync();
        }
        public virtual async Task AddUserToRolesAsync(ApplicationUser user, IdentityRole[] roles)
        {
            foreach (IdentityRole role in roles)
            {
                await UserManager.AddToRoleAsync(user, role.Name);
            }
        }
        public virtual async Task RemoveUserFromRolesAsync(ApplicationUser user, IdentityRole[] roles)
        {
            foreach (IdentityRole role in roles)
            {
                await UserManager.RemoveFromRoleAsync(user, role.Name);
            }
        }

        public async Task SetupRolesAsync()
        {
            foreach (string role in Models.Roles.All)
            {
                if (!await RoleExistsAsync(role))
                {
                    await CreateRoleAsync(role);
                }
            }
        }
        #endregion

        #region Statistics
        public virtual async Task<UserStatistics> GetStatisticsAsync()
        {
            int totalUsers = await _db.Users.CountAsync();
            int totalAdmins = (await GetUsersInRole("Admin")).Count;
            var data = await _db.Users.Where(p => p.CreatedOn >= DateTime.Now.AddYears(-1)).Select(c => new { date = c.CreatedOn.Date, month = c.CreatedOn.Month }).ToListAsync();

            var createdByDate = data.GroupBy(p => p.date).Select(g => new { name = g.Key, count = g.Count() });
            var createdByMonth = data.GroupBy(p => p.month).Select(g => new { name = g.Key, count = g.Count() });

            List<KeyValuePair<string, int>> days = new List<KeyValuePair<string, int>>();
            foreach (DateTime day in DateTimeExtensions.EachDay(DateTime.UtcNow.AddDays(-89), DateTime.UtcNow))
            {
                var dayvalue = createdByDate.SingleOrDefault(c => c.name == day.Date);
                int count = dayvalue != null ? dayvalue.count : 0;
                days.Add(new KeyValuePair<string, int>(day.ToString("dd MMM"), count));

            }

            List<KeyValuePair<string, int>> months = new List<KeyValuePair<string, int>>();
            for (DateTime dt = DateTime.UtcNow.AddMonths(-11); dt <= DateTime.UtcNow; dt = dt.AddMonths(1))
            {
                var monthvalue = createdByMonth.SingleOrDefault(c => c.name == dt.Month);
                int count = monthvalue != null ? monthvalue.count : 0;
                months.Add(new KeyValuePair<string, int>(dt.ToString("dd MMM"), count));
            }

            return new UserStatistics(totalUsers, totalAdmins, days, months);
        }
        #endregion

    }
}
