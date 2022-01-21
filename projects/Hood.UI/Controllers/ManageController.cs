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
using Microsoft.AspNetCore.Authentication.Cookies;
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
    [Authorize(Hood.Identity.Policies.Active)]
    public abstract class ManageController : ManageController<HoodDbContext>
    {
        public ManageController() : base() { }
    }

    [Authorize(Hood.Identity.Policies.Active)]
    public abstract class ManageController<TContext> : BaseController<TContext, ApplicationUser, IdentityRole>
         where TContext : HoodDbContext
    {
        public ManageController()
            : base()
        { }

        [HttpGet]
        [Route("account/manage")]
        public virtual async Task<IActionResult> Index(string r)
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
                Profile = await _account.GetUserProfileByIdAsync(user.Id),
                Accounts = user.ConnectedAuth0Accounts,
                NewAccount = r == "new-account-connection"
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

                User.SetUserClaims(model.Profile);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);

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
                    var directory = await _account.GetDirectoryAsync(User.GetLocalUserId());
                    mediaResult = await _media.ProcessUpload(file, _directoryManager.GetPath(directory.Id));
                    user.Avatar = mediaResult;
                    await _account.UpdateUserAsync(user);
                    _db.Media.Add(new MediaObject(mediaResult, directory.Id));
                    await _db.SaveChangesAsync();

                    User.SetUserClaims(user);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);
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


    }
}
