﻿using Hood.Core;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Hood.Services
{
    public class Auth0AccountRepository : AccountRepository, IAccountRepository
    {
        public Auth0AccountRepository() : base()
        { }

        #region Helpers        
        protected override IQueryable<ApplicationUser> UserQuery
        {
            get
            {
                IQueryable<ApplicationUser> query = _db.Users
                    .Include(u => u.ConnectedAuth0Accounts)
                    .Include(u => u.Addresses);
                return query;
            }
        }
        #endregion

        #region Account stuff         
        public override async Task<ApplicationUser> GetUserByAuth0Id(string userId)
        {
            var auth0user = await _db.Auth0Users.Include(au => au.User).SingleOrDefaultAsync(au => au.Id == userId);
            if (auth0user != null)
            {
                return auth0user.User;
            }
            return null;
        }
        public override async Task<UserProfile> GetUserProfileByIdAsync(string id)
        {
            var auth0user = await GetUserByAuth0Id(id);
            if (auth0user != null)
            {
                return await base.GetUserProfileByIdAsync(auth0user.Id);
            }
            return null;
        }
        public override async Task UpdateUserAsync(ApplicationUser user)
        {
#warning Auth0 - UpdateUserAsync - send relevant changes to Auth0
            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }
        public override Task DeleteUserAsync(string userId, System.Security.Claims.ClaimsPrincipal adminUser)
        {
#warning Auth0 - DeleteUserAsync
            throw new NotImplementedException();
        }
        public override Task SendVerificationEmail(ApplicationUser user)
        {
            throw new ApplicationException("This feature is disabled when using Auth0.");
        }
        public override Task<IdentityResult> ChangePassword(ApplicationUser user, string oldPassword, string newPassword)
        {
            throw new ApplicationException("This feature is disabled when using Auth0.");
        }
        public override Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string code, string password)
        {
            throw new ApplicationException("This feature is disabled when using Auth0.");
        }
        public override Task SendPasswordResetToken(ApplicationUser user)
        {
            throw new ApplicationException("This feature is disabled when using Auth0.");
        }
        public override async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            try
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new List<IdentityError>() {
                    new IdentityError() {
                        Code = "CreateFailed",
                        Description = "Could not create the user account on the data store: " + ex.Message
                    }
                }.ToArray());
            }
        }
        #endregion

        #region Profiles 
        public override Task SetEmailAsync(ApplicationUser modelToUpdate, string email)
        {
#warning Auth0 - SetEmailAsync
            throw new NotImplementedException();
        }
        public override Task SetPhoneNumberAsync(ApplicationUser modelToUpdate, string phoneNumber)
        {
#warning Auth0 - SetPhoneNumberAsync
            throw new NotImplementedException();
        }
        #endregion

        #region Roles
        public override Task<IList<ApplicationUser>> GetUsersInRole(string roleName)
        {
#warning Auth0 - GetUsersInRole
            throw new NotImplementedException();
        }
        public override Task<IList<string>> GetRolesForUser(ApplicationUser user)
        {
#warning Auth0 - GetRolesForUser
            throw new NotImplementedException();
        }
        public override Task<bool> RoleExistsAsync(string role)
        {
#warning Auth0 - RoleExistsAsync
            throw new NotImplementedException();
        }
        public override Task CreateRoleAsync(IdentityRole identityRole)
        {
#warning Auth0 - CreateRoleAsync
            throw new NotImplementedException();
        }

        #endregion

        #region Addresses

        #endregion

        #region Statistics

        #endregion
    }
}
