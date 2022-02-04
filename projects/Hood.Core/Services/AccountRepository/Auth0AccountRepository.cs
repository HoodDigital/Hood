using Hood.Core;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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

        #region Account stuff         
        public override async Task UpdateUserAsync(ApplicationUser user)
        {
            _db.Entry(user).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }
        public override async Task DeleteUserAsync(string localUserId, System.Security.Claims.ClaimsPrincipal adminUser)
        {
            // go through all the auth0 accounts and remove them from the system.
            var accounts = await _db.Auth0Users.Where(u => u.LocalUserId == localUserId).ToListAsync();
            var auth0Service = new Auth0Service();
            foreach (var account in accounts)
            {
                //remove from auth0
                if (Engine.Settings.Account.DeleteRemoteAccounts)
                {
                    await auth0Service.DeleteUser(account.Id);
                }
                _db.Entry(account).State = EntityState.Deleted;
            }
            await _db.SaveChangesAsync();

            var user = await PrepareUserForDelete(localUserId, adminUser);

            _db.Auth0Users.Where(l => l.LocalUserId == localUserId).ForEach(f => _db.Entry(f).State = EntityState.Deleted);
            await _db.SaveChangesAsync();

            // now delete the user
            await base.DeleteUserAsync(localUserId, adminUser);
        }
        public override async Task SendVerificationEmail(ApplicationUser localUser, string userId, string returnUrl)
        {
            // get the users' current connected account.
            // send a verification email on the whattheolddowntheold.
            var authService = new Auth0Service();
            var ticket = await authService.GetEmailVerificationTicket(userId, returnUrl);
            var verifyModel = new VerifyEmailModel(localUser, ticket.Value)
            {
                SendToRecipient = true
            };
            await _mailService.ProcessAndSend(verifyModel);
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
            return await base.CreateAsync(user, password);
        }
        #endregion

        #region Profiles 
        public override async Task SetEmailAsync(ApplicationUser modelToUpdate, string email)
        {
            await base.SetEmailAsync(modelToUpdate, email);
        }
        public override async Task SetPhoneNumberAsync(ApplicationUser modelToUpdate, string phoneNumber)
        {
            await base.SetPhoneNumberAsync(modelToUpdate, phoneNumber);
        }
        #endregion

        #region Roles
        public async override Task<ApplicationRole> CreateRoleAsync(string role)
        {
            var internalRole = await base.CreateRoleAsync(role);
            var authService = new Auth0Service();
            var remoteRole = await authService.CreateRoleAsync(internalRole);
            internalRole.RemoteId = remoteRole.Id;
            _db.Roles.Update(internalRole);
            await _db.SaveChangesAsync();
            return internalRole;
        }
        public async override Task DeleteRoleAsync(string role)
        {
            if (Engine.Settings.Account.DeleteRemoteAccounts)
            {
                var authService = new Auth0Service();
                await authService.DeleteRole(role);
            }
            await base.DeleteRoleAsync(role);
        }
        #endregion

        #region Addresses

        #endregion

        #region Statistics

        #endregion
    }
}
