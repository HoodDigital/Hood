using Auth0.AspNetCore.Authentication;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using Hood.Attributes;
using Hood.BaseTypes;
using Hood.Constants.Identity;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Identity;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Unsplasharp;

namespace Hood.BaseControllers
{
    public abstract class AccountController : BaseController
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly IPasswordAccountRepository _account;
        public AccountController()
        {
            _userManager = Engine.Services.Resolve<UserManager<ApplicationUser>>();
            _account = Engine.Services.Resolve<IPasswordAccountRepository>();
        }

        // #region Account Home

        [HttpGet]
        [Route("account/")]
        public virtual async Task<IActionResult> Index(string returnUrl, bool created = false)
        {
            var user = await GetCurrentUserOrThrow();
            var model = new ManageAccountViewModel
            {
                LocalUserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                IsEmailConfirmed = user.EmailConfirmed,
                StatusMessage = SaveMessage,
                Avatar = user.UserProfile.Avatar,
                Profile = await _account.GetUserProfileByIdAsync(user.Id),
                ReturnUrl = returnUrl,
                NewAccountCreated = created
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/")]
        public virtual async Task<IActionResult> Index(ManageAccountViewModel model, string returnUrl, bool created = false)
        {
            try
            {
                var user = await GetCurrentUserOrThrow();
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                user = await _account.UpdateProfileAsync(user, model.Profile);

                User.SetUserClaims(user.UserProfile);

                var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
                await signInManager.SignInAsync(user, isPersistent: false);

                SaveMessage = "Your profile has been updated.";
                MessageType = AlertType.Success;
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                SaveMessage = "Something went wrong: " + ex.Message;
                MessageType = AlertType.Danger;
                model.ReturnUrl = returnUrl;
                model.NewAccountCreated = created;
                return View(model);
            }

        }

        // [Route("account/avatar")]
        // public virtual async Task<Response> UploadAvatar(IFormFile file, string userId)
        // {
        //     // User must have an organisation.
        //     try
        //     {

        //         var user = await GetCurrentUserOrThrow();
        //         IMediaObject mediaResult = null;
        //         if (file != null)
        //         {
        //             // If the club already has an avatar, delete it from the system.
        //             if (user.Avatar != null)
        //             {
        //                 var mediaItem = await _db.Media.SingleOrDefaultAsync(m => m.UniqueId == user.Avatar.UniqueId);
        //                 if (mediaItem != null)
        //                     _db.Entry(mediaItem).State = EntityState.Deleted;
        //                 await _media.DeleteStoredMedia((MediaObject)user.Avatar);
        //             }
        //             var directory = await _account.GetDirectoryAsync(User.GetLocalUserId());
        //             mediaResult = await _media.ProcessUpload(file, _directoryManager.GetPath(directory.Id));
        //             user.Avatar = mediaResult;
        //             await _account.UpdateUserAsync(user);
        //             _db.Media.Add(new MediaObject(mediaResult, directory.Id));
        //             await _db.SaveChangesAsync();

        //             User.SetUserClaims(user);
        //             await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);
        //         }
        //         return new Response(true, mediaResult, $"The media has been set for attached successfully.");
        //     }
        //     catch (Exception ex)
        //     {
        //         return await ErrorResponseAsync<AccountController>($"There was an error setting the avatar.", ex);
        //     }
        // }

        // #endregion

        #region Login 

        [HttpGet]
        [AllowAnonymous]
        [Route("account/login")]
        public virtual IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("Login");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("account/login")]
        public virtual async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            AccountSettings accountSettings = Engine.Settings.Account;
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {

                Services.RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
                if (!recaptcha.Passed)
                {
                    ModelState.AddModelError(string.Empty, "You have failed to pass the reCaptcha check. Please refresh your page and try again.");
                    return View(model);
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true                
                var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
                Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    ApplicationUser user = await _userManager.FindByNameAsync(model.Username);
                    if (Engine.Settings.Account.RequireEmailConfirmation && !user.EmailConfirmed)
                    {
                        await SendVerificationEmail(user, User.GetUserId(), Url.AbsoluteAction("Login", "Account"));
                        return RedirectToAction(nameof(ConfirmRequired), new { user = user.Id });
                    }

                    user.LastLogOn = DateTime.UtcNow;
                    user.LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString();
                    user.LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString();
                    await _account.UpdateUserAsync(user);

                    await _logService.AddLogAsync<AccountController>($"User ({model.Username}) logged in.");

                    return RedirectToLocal(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Registration

        [HttpGet]
        [AllowAnonymous]
        [Route("account/register")]
        public virtual IActionResult Register(string returnUrl = null)
        {
            AccountSettings accountSettings = Engine.Settings.Account;
            if (!accountSettings.EnableRegistration)
            {
                return RedirectToAction(nameof(RegistrationClosed));
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("account/register")]
        public virtual async Task<IActionResult> Register(PasswordRegisterViewModel model, string returnUrl = null)
        {
            AccountSettings accountSettings = Engine.Settings.Account;
            if (!accountSettings.EnableRegistration)
            {
                return RedirectToAction(nameof(RegistrationClosed));
            }

            ViewData["ReturnUrl"] = returnUrl;

            if (!model.Consent)
            {
                ModelState.AddModelError(string.Empty, "You did not give consent for us to store your data, therefore we cannot complete the signup process.");
            }

            if (ModelState.IsValid)
            {

                Services.RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
                if (!recaptcha.Passed)
                {
                    ModelState.AddModelError(string.Empty, "You have failed to pass the reCaptcha check. Please refresh your page and try again.");
                    return View(model);
                }

                ApplicationUser user = new ApplicationUser
                {
                    UserName = model.Username.IsSet() ? model.Username : model.Email,
                    Email = model.Email,
                    PhoneNumber = model.Phone,
                    CreatedOn = DateTime.UtcNow,
                    LastLogOn = DateTime.UtcNow,
                    LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString(),
                    LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString(),
                    UserProfile = new UserProfile
                    {
                        UserName = model.Username.IsSet() ? model.Username : model.Email,
                        Email = model.Email,
                        PhoneNumber = model.Phone,

                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        DisplayName = model.DisplayName,
                        JobTitle = model.JobTitle,
                        Anonymous = model.Anonymous
                    }
                };
                IdentityResult result = await _account.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SendWelcomeEmail(user);

                    user.Active = !Engine.Settings.Account.RequireEmailConfirmation;
                    user.EmailConfirmed = !Engine.Settings.Account.RequireEmailConfirmation;
                    user.LastLogOn = DateTime.UtcNow;
                    user.LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString();
                    user.LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString();

                    await _account.UpdateUserAsync(user);

                    var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
                    await signInManager.SignInAsync(user, isPersistent: false);


                    if (Engine.Settings.Account.RequireEmailConfirmation)
                    {
                        await SendVerificationEmail(user, User.GetUserId(), Url.AbsoluteAction("Login", "Account"));
                        return RedirectToAction(nameof(AccountController.ConfirmRequired), "Account");
                    }
                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        #endregion

        #region Logout

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("account/logout")]
        public virtual async Task<IActionResult> LogOff(string returnUrl = "/")
        {
            System.Security.Principal.IIdentity user = User.Identity;
            var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
            await signInManager.SignOutAsync();
            await _logService.AddLogAsync<AccountController>($"User ({user.Name}) logged out.");
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Lockout / Access Denied / Registration Closed

        [HttpGet]
        [AllowAnonymous]
        [Route("account/registration-closed")]
        public virtual IActionResult RegistrationClosed(string returnUrl = null)
        {
            AccountSettings accountSettings = Engine.Settings.Account;
            if (accountSettings.EnableRegistration)
            {
                return RedirectToAction(nameof(Register), new { returnUrl });
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/lockout")]
        public virtual IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/access-denied")]
        public virtual IActionResult AccessDenied(string returnUrl)
        {
            if (User.Identity.IsAuthenticated && !User.IsActive())
            {
                return RedirectToAction(nameof(ConfirmRequired));
            }
            Response.StatusCode = 403;
            return View();
        }

        #endregion

        #region Confirm Email

        [HttpGet]
        [Authorize]
        [Route("account/email/confirm-required")]
        public virtual IActionResult ConfirmRequired()
        {
            return View(new ConfirmRequiredModel());
        }

        [HttpGet]
        [Authorize]
        [Route("account/email/confirm")]
        public virtual async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            ApplicationUser user = await _account.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }

            IdentityResult result = await _account.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                throw new Exception("Your email address could not be confirmed, the link you have clicked is invalid, perhaps it has expired. You can log in to resend a new verification email.");
            }

            if (user.Active)
            {
                if (User.Identity.IsAuthenticated)
                {
                    SaveMessage = "Your email address has been successfully validated.";
                    RedirectToAction(nameof(Index), "Manage");
                }
            }
            else
            {
                user.Active = true;
                await _account.UpdateUserAsync(user);
            }
            return View("ConfirmEmail");
        }

        [HttpGet]
        [Authorize]
        [Route("account/email/resend-confirmation")]
        public virtual async Task<IActionResult> ResendConfirm()
        {
            try
            {

                var user = await GetCurrentUserOrThrow();
                await SendVerificationEmail((ApplicationUser)user, User.GetUserId(), Url.AbsoluteAction("Login", "Account"));
                SaveMessage = $"Email verification has been resent.";
                MessageType = AlertType.Success;

            }
            catch (Exception ex)
            {
                SaveMessage = $"Error sending an email verification: {ex.Message}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<AccountController>($"Error when sending an email verification.", ex);
            }

            return RedirectToAction(nameof(ConfirmRequired));
        }

        #endregion

        #region Change Password

        [HttpGet]
        [Authorize(Policies.Active)]
        [Route("account/change-password")]
        public virtual IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize(Policies.Active)]
        [ValidateAntiForgeryToken]
        [Route("account/change-password")]
        public virtual async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserOrThrow();
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

            await _logService.AddLogAsync<AccountController>($"User ({user.UserName}) changed their password successfully.");

            MessageType = AlertType.Success;
            SaveMessage = "Your password has been changed.";

            return RedirectToAction(nameof(ChangePassword));
        }

        #endregion

        #region Forgot / Reset Password

        [HttpGet]
        [AllowAnonymous]
        [Route("account/forgot-password")]
        public virtual IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("account/forgot-password")]
        public virtual async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _account.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                await SendPasswordResetToken(user);

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/forgot-password/confirm")]
        public virtual IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/reset-password")]
        public virtual IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            ResetPasswordViewModel model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("account/reset-password")]
        public virtual async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            ApplicationUser user = await _account.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            IdentityResult result = await _account.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/reset-password/confirm")]
        public virtual IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        #endregion

        #region Delete Account

        [HttpGet]
        [Authorize]
        [Route("account/delete")]
        public virtual IActionResult Delete()
        {
            return View(nameof(Delete), new SaveableModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("account/delete/confirm")]
        public virtual async Task<IActionResult> ConfirmDelete()
        {
            try
            {

                var user = await GetCurrentUserOrThrow();
                await _account.DeleteUserAsync(user.Id, User);

                var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
                await signInManager.SignOutAsync();

                await _logService.AddLogAsync<AccountController>($"User with Id {user.Id} has deleted their account.");
                return RedirectToAction(nameof(Deleted));
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error deleting your account: {ex.Message}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<AccountController>($"Error when user attemted to delete their account.", ex);
            }
            return RedirectToAction(nameof(Delete));
        }

        [AllowAnonymous]
        [Route("account/deleted")]
        public virtual IActionResult Deleted()
        {
            return View(nameof(Deleted));
        }

        #endregion

        #region Helpers

        protected virtual async Task<ApplicationUser> GetCurrentUserOrThrow()
        {
            var user = await _account.GetUserByIdAsync(User.GetLocalUserId());
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with email '{User.GetEmail()}'.");
            }
            return user;
        }

        protected virtual async Task SendVerificationEmail(ApplicationUser localUser, string userId, string returnUrl)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(localUser);
            var contextAccessor = Engine.Services.Resolve<IHttpContextAccessor>();
            if (contextAccessor == null)
            {
                return;
            }
            var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
            if (linkGenerator == null)
            {
                return;
            }
            var callbackUrl = linkGenerator.GetUriByAction(contextAccessor.HttpContext, "ConfirmEmail", "Account", new { userId = localUser.Id, code, returnUrl });
            var verifyModel = new VerifyEmailModel(localUser.UserProfile, callbackUrl)
            {
                SendToRecipient = true
            };
            await _mailService.ProcessAndSend(verifyModel);
        }

        protected virtual async Task SendPasswordResetToken(ApplicationUser user)
        {
            string code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var contextAccessor = Engine.Services.Resolve<IHttpContextAccessor>();
            if (contextAccessor == null)
            {
                return;
            }
            var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
            if (linkGenerator == null)
            {
                return;
            }

            string callbackUrl = linkGenerator.GetUriByAction(contextAccessor.HttpContext, "ResetPassword", "Account", new { userId = user.Id, code });

            MailObject message = new MailObject()
            {
                To = new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                PreHeader = "Reset your password.",
                Subject = "Reset your password."
            };
            message.AddParagraph($"Please reset your password by clicking here:");
            message.AddCallToAction("Reset your password", callbackUrl);
            message.Template = MailSettings.WarningTemplate;
            await _emailSender.SendEmailAsync(message);
        }

        protected virtual async Task SendWelcomeEmail(ApplicationUser user)
        {
            string loginLink = Url.Action("Login", "Account", null, protocol: HttpContext.Request.Scheme);
            WelcomeEmailModel welcomeModel = new WelcomeEmailModel(user.UserProfile, loginLink)
            {
                SendToRecipient = true,
                NotifyRole = Engine.Settings.Account.NotifyNewAccount ? "NewAccountNotifications" : null
            };
            await _mailService.ProcessAndSend(welcomeModel);
        }

        protected virtual void AddErrors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        protected virtual IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion
    }
}