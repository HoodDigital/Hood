using Hood.Attributes;
using Hood.BaseTypes;
using Hood.Caching;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    [Authorize]
    public abstract class ManageController : ManageController<HoodDbContext>
    {
        public ManageController() : base() { }
    }

    [Authorize]
    public abstract class ManageController<TContext> : BaseController<TContext, ApplicationUser, IdentityRole>
         where TContext : HoodDbContext
    {
        public ManageController()
            : base()
        { }

        [HttpGet]
        [Route("account/manage")]
        public virtual async Task<IActionResult> Index()
        {
            ApplicationUser user = await GetCurrentUserOrThrow();
            var model = new UserViewModel
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsEmailConfirmed = user.EmailConfirmed,
                StatusMessage = SaveMessage,
                Avatar = user.Avatar,
                Profile = User.GetUserProfile(),
                Roles = await _account.GetRolesForUser(user)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/manage")]
        public virtual async Task<IActionResult> Index(UserViewModel model)
        {
            ApplicationUser user = await GetCurrentUserOrThrow();
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Roles = await _account.GetRolesForUser(user);
                    return View(model);
                }

                var email = user.Email;
                if (model.Email != email)
                {
                    await _account.SetEmailAsync(user, model.Email);
                }

                var phoneNumber = user.PhoneNumber;
                if (model.PhoneNumber != phoneNumber)
                {
                    await _account.SetPhoneNumberAsync(user, model.PhoneNumber);
                }

                model.Profile.Id = user.Id;
                model.Profile.Notes = user.Notes;
                await _account.UpdateProfileAsync(model.Profile);

                SaveMessage = "Your profile has been updated.";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = "Something went wrong: " + ex.Message;
                MessageType = AlertType.Danger;
            }

            return RedirectToAction(nameof(Index));
        }

        [Route("account/manage/avatar")]
        public virtual async Task<Response> UploadAvatar(IFormFile file, string userId)
        {
            // User must have an organisation.
            try
            {
                ApplicationUser user = await GetCurrentUserOrThrow();
                IMediaObject mediaResult = null;
                if (file != null)
                {
                    // If the club already has an avatar, delete it from the system.
                    if (user.Avatar != null)
                    {
                        var mediaItem = await _db.Media.SingleOrDefaultAsync(m => m.UniqueId == user.Avatar.UniqueId);
                        if (mediaItem != null)
                            _db.Entry(mediaItem).State = EntityState.Deleted;
                        await _media.DeleteStoredMedia((MediaObject)user.Avatar);
                    }
                    var directory = await Engine.AccountManager.GetDirectoryAsync(User.GetUserId());
                    mediaResult = await _media.ProcessUpload(file, _directoryManager.GetPath(directory.Id));
                    user.Avatar = mediaResult;
                    await _account.UpdateUserAsync(user);
                    _db.Media.Add(new MediaObject(mediaResult, directory.Id));
                    await _db.SaveChangesAsync();
                }
                return new Response(true, mediaResult, $"The media has been set for attached successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<ManageController>($"There was an error setting the avatar.", ex);
            }
        }

        [Route("account/manage/verify-email")]
        public virtual async Task<IActionResult> SendVerificationEmail()
        {

            ApplicationUser user = await GetCurrentUserOrThrow();
            await _account.SendVerificationEmail(user);

            SaveMessage = "Verification email sent. Please check your email.";
            MessageType = AlertType.Success;

            if (user.Active)
                return RedirectToAction(nameof(Index));
            else
                return RedirectToAction(nameof(AccountController.ConfirmRequired), "Account");
        }

        [HttpGet]
        [Route("account/manage/change-password")]
        public virtual async Task<IActionResult> ChangePassword()
        {
            ApplicationUser user = await GetCurrentUserOrThrow();
            if (Engine.Auth0Enabled)
            {
#warning Auth0 - ChangePassword - send reset link and redirect back to manage page.
                throw new NotImplementedException();
            }
            var model = new ChangePasswordViewModel { StatusMessage = SaveMessage };
            return View(model);
        }

        [HttpPost]
        [DisableForAuth0]
        [ValidateAntiForgeryToken]
        [Route("account/manage/change-password")]
        public virtual async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            ApplicationUser user = await GetCurrentUserOrThrow();
            var changePasswordResult = await _account.ChangePassword(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
            await signInManager.SignInAsync(user, isPersistent: false);

            await _logService.AddLogAsync<ManageController>($"User ({user.UserName}) changed their password successfully.");
            SaveMessage = "Your password has been changed.";

            return RedirectToAction(nameof(ChangePassword));
        }

        [HttpGet]
        [Route("account/delete")]
        public virtual IActionResult Delete()
        {
            return View(nameof(Delete), new SaveableModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/delete/confirm")]
        public virtual async Task<IActionResult> ConfirmDelete()
        {
            try
            {
                ApplicationUser user = await GetCurrentUserOrThrow();
                await _account.DeleteUserAsync(user.Id, User);

                if (Engine.Auth0Enabled)
                {
#warning Auth0 - ConfirmDelete - log user out and redirect back to deleted page. 
                    throw new NotImplementedException();
                }
                else
                {
                    var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
                    await signInManager.SignOutAsync();
                }

                await _logService.AddLogAsync<ManageController>($"User with Id {user.Id} has deleted their account.");
                return RedirectToAction(nameof(Deleted));
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error deleting your account: {ex.Message}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<ManageController>($"Error when user attemted to delete their account.", ex);
            }
            return RedirectToAction(nameof(Delete));
        }

        [AllowAnonymous]
        [Route("account/deleted")]
        public virtual IActionResult Deleted()
        {
            return View(nameof(Deleted));
        }

    }
}
