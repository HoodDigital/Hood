using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Http;
using System;
using Hood.Extensions;

namespace Hood.Controllers
{
    [Authorize]
    //[Area("Hood")]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly IAccountRepository _auth;
        private readonly IMediaManager<SiteMedia> _media;

        public ManageController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender,
        ISmsSender smsSender,
        IAccountRepository auth,
        ILoggerFactory loggerFactory,
        IMediaManager<SiteMedia> media)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _auth = auth;
            _logger = loggerFactory.CreateLogger<ManageController>();
            _media = media;
        }

        //
        // GET: /Manage/Index
        [HttpGet]
        [Route("account/manage/")]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            string successFormat = "<div class=\"alert alert-success\"><p><i class=\"fa fa-exclamation-triangle m-r-xs\"></i>{0}</p></div>";
            string errorFormat = "<div class=\"alert alert-success\"><p><i class=\"fa fa-exclamation-triangle m-r-xs\"></i>{0}</p></div>";
            string warningFormat = "<div class=\"alert alert-success\"><p><i class=\"fa fa-exclamation-triangle m-r-xs\"></i>{0}</p></div>";

            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? string.Format(successFormat, "Your password has been changed.")
                : message == ManageMessageId.SetPasswordSuccess ? string.Format(successFormat, "Your password has been set.")
                : message == ManageMessageId.SetTwoFactorSuccess ? string.Format(successFormat, "Your two-factor authentication provider has been set.")
                : message == ManageMessageId.Error ? string.Format(errorFormat, "An error has occurred.")
                : message == ManageMessageId.AddPhoneSuccess ? string.Format(successFormat, "Your phone number was added.")
                : message == ManageMessageId.RemovePhoneSuccess ? string.Format(warningFormat, "Your phone number was removed.")
                : message == ManageMessageId.AddNewAddressSuccess ? string.Format(successFormat, "Your new address has been added.")
                : message == ManageMessageId.DeleteAddressFailure ? string.Format(errorFormat, "Your address could not be removed, please try again.")
                : message == ManageMessageId.DeleteAddressSuccess ? string.Format(warningFormat, "Your address has been removed.")
                : message == ManageMessageId.UpdateBilling ? string.Format(successFormat, "Your delivery address was updated.")
                : message == ManageMessageId.UpdateDelivery ? string.Format(successFormat, "Your billing address was updated.")
                : "";

            var user = await GetCurrentUserAsync();
            var model = new IndexViewModel
            {
                User = user,
                HasPassword = await _userManager.HasPasswordAsync(user),
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
                TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
                Roles = await _userManager.GetRolesAsync(user),
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user)
            };
            return View(model);
        }

        [HttpGet]
        [Route("account/profile/")]
        public async Task<IActionResult> Profile()
        {
            EditUserModel um = new EditUserModel()
            {
                User = _auth.GetUserById(_userManager.GetUserId(User))
            };
            um.Roles = await _userManager.GetRolesAsync(um.User);
            um.AllRoles = _auth.GetAllRoles();
            return View(um);
        }

        [HttpPost]
        [Route("account/profile/")]
        public IActionResult Profile(EditUserModel model)
        {
            EditUserModel um = new EditUserModel()
            {
                User = _auth.GetUserById(_userManager.GetUserId(User))
            };
            try
            {
                model.User.CopyProperties(um.User);
                _auth.UpdateUser(um.User);
                model.SaveMessage = "Saved!";
                model.MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                model.SaveMessage = "An error occurred while saving: " + ex.Message;
                model.MessageType = Enums.AlertType.Danger;
            }
            return View(model);
        }

        [Route("account/upload/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file, string userId)
        {
            // User must have an organisation.
            var user = _auth.GetUserById(userId);
            if (user == null)
                return NotFound();

            try
            {
                SiteMedia mediaResult = null;
                if (file != null)
                {
                    // If the club already has an avatar, delete it from the system.
                    if (user.Avatar != null)
                    {
                        await _media.DeleteStoredMedia(user.Avatar);
                    }
                    mediaResult = await _media.ProcessUpload(file, new SiteMedia() { Directory = string.Format("users/{0}/", userId) });
                    user.Avatar = mediaResult;
                    _auth.UpdateUser(user);
                }
                return Json(new { Success = true, Image = mediaResult });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.InnerException != null ? ex.InnerException.Message : ex.Message });
            }
        }


        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/manage/logins/remove/")]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
        {
            ManageMessageId? message = ManageMessageId.Error;
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    message = ManageMessageId.RemoveLoginSuccess;
                }
            }
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        [Route("account/manage/phone-numbers/remove/")]
        public IActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/manage/phone-numbers/add/")]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var user = await GetCurrentUserAsync();
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
            await _smsSender.SendSmsAsync(model.PhoneNumber, "Your security code is: " + code);
            return RedirectToAction(nameof(VerifyPhoneNumber), new { PhoneNumber = model.PhoneNumber });
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        [HttpGet]
        [Route("account/manage/phone-numbers/verify/")]
        public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(await GetCurrentUserAsync(), phoneNumber);
            // Send an SMS to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/manage/phone-numbers/verify/")]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.Code);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.AddPhoneSuccess });
                }
            }
            // If we got this far, something failed, redisplay the form
            ModelState.AddModelError(string.Empty, "Failed to verify phone number");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/manage/phone-numbers/remove/")]
        public async Task<IActionResult> RemovePhoneNumber()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.SetPhoneNumberAsync(user, null);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.RemovePhoneSuccess });
                }
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/manage/two-factor-auth/enable")]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(1, "User enabled two-factor authentication.");
            }
            return RedirectToAction(nameof(Index), "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/manage/two-factor-auth/disable")]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(2, "User disabled two-factor authentication.");
            }
            return RedirectToAction(nameof(Index), "Manage");
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        [Route("account/change-password/")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/change-password/")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed their password successfully.");
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //
        // GET: /Manage/SetPassword
        [HttpGet]
        [Route("account/set-password/")]
        public IActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/set-password/")]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //GET: /Manage/ManageLogins
        [HttpGet]
        [Route("account/manage/logins/")]
        public async Task<IActionResult> ManageLogins(ManageMessageId? message = null)
        {
            ViewData["StatusMessage"] =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.AddLoginSuccess ? "The external login was added."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await _userManager.GetLoginsAsync(user);
            var otherLogins = _signInManager.GetExternalAuthenticationSchemes().Where(auth => userLogins.All(ul => auth.AuthenticationScheme != ul.LoginProvider)).ToList();
            ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/manage/logins/link/")]
        public IActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action("LinkLoginCallback", "Manage");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return Challenge(properties, provider);
        }

        //
        // GET: /Manage/LinkLoginCallback
        [HttpGet]
        [Route("account/manage/logins/link/callback/")]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if (info == null)
            {
                return RedirectToAction(nameof(ManageLogins), new { Message = ManageMessageId.Error });
            }
            var result = await _userManager.AddLoginAsync(user, info);
            var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;
            return RedirectToAction(nameof(ManageLogins), new { Message = message });
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            AddLoginSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            AddNewAddressSuccess,
            DeleteAddressSuccess,
            DeleteAddressFailure,
            UpdateBilling,
            UpdateDelivery,
            Error
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        #endregion
    }
}
