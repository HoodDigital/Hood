using Auth0.AspNetCore.Authentication;
using Hood.Attributes;
using Hood.BaseTypes;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
using Hood.Identity;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Unsplasharp;

namespace Hood.Controllers
{
    public abstract class AccountController : AccountController<HoodDbContext>
    {
        public AccountController() : base() { }
    }

    public abstract class AccountController<TContext> : BaseController<TContext, ApplicationUser, IdentityRole>
         where TContext : HoodDbContext
    {
        public AccountController()
            : base()
        { }

        #region Password Login

        [HttpGet]
        [AllowAnonymous]
        [Route("account/login")]
        public virtual IActionResult Login(string returnUrl = null)
        {
            if (Engine.Auth0Enabled)
            {
                return RedirectToAction(nameof(Authorize), new { returnUrl });
            }
            else
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View("Login");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [DisableForAuth0]
        [ValidateAntiForgeryToken]
        [Route("account/login")]
        public virtual async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            AccountSettings accountSettings = Engine.Settings.Account;
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {

                Services.RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
                if (!recaptcha.Success)
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
                    ApplicationUser user = await _account.GetUserByEmailAsync(model.Username);
                    if (Engine.Settings.Account.RequireEmailConfirmation && !user.EmailConfirmed)
                    {
                        await _account.SendVerificationEmail(user);
                        return RedirectToAction(nameof(ConfirmRequired), new { user = user.Id });
                    }

                    user.LastLogOn = DateTime.UtcNow;
                    user.LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString();
                    user.LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString();
                    await _account.UpdateUserAsync(user);

                    await _logService.AddLogAsync<AccountController<TContext>>($"User ({model.Username}) logged in.");

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

<<<<<<< HEAD
=======
        #region Magic Link Login

        [HttpGet]
        [AllowAnonymous]
        [Route("account/login/generate")]
        public virtual IActionResult MagicLoginGenerate()
        {
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("account/login/generate")]
        public virtual async Task<IActionResult> MagicLoginGenerate(MagicLoginViewModel model, string returnUrl = null)
        {
            AccountSettings accountSettings = Engine.Settings.Account;
            if (!accountSettings.MagicLinkLogin)
            {
                return RedirectToAction(nameof(Login));
            }

            ViewData["ReturnUrl"] = returnUrl;
            try
            {
                if (ModelState.IsValid)
                {

                    Services.RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
                    if (!recaptcha.Success)
                    {
                        ModelState.AddModelError(string.Empty, "You have failed to pass the reCaptcha check. Please refresh your page and try again.");
                        return View("MagicLogin", model);
                    }

                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true

                    var user = await _userManager.FindByEmailAsync(model.Email);

                    if (user == null)
                    {
                        throw new Exception("Your email address was not recognised.");
                    }

                    var token = await _userManager.GenerateUserTokenAsync(user, "MagicLoginTokenProvider", "hood-login");

                    string callbackUrl = Url.Action("MagicLogin", "Account", new { u = user.Id, t = token, r = returnUrl }, protocol: HttpContext.Request.Scheme);

                    MailObject message = new MailObject()
                    {
                        To = new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                        PreHeader = $"Log in to {Engine.Settings.Basic.Title}",
                        Subject = $"Log in to {Engine.Settings.Basic.Title}"
                    };
                    message.AddParagraph($"Click the button below to log into your account.");
                    message.AddCallToAction("Log in", callbackUrl);
                    message.Template = MailSettings.PlainTemplate;
                    await _emailSender.SendEmailAsync(message);

                    return RedirectToAction(nameof(MagicLoginSent));

                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            // If we got this far, something failed, redisplay form
            return View("MagicLogin", model);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/login/sent")]
        public virtual IActionResult MagicLoginSent(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [Route("account/login/code")]
        public virtual async Task<IActionResult> MagicLogin(string t, string u, string r = "/")
        {
            try
            {
                var user = await _userManager.FindByIdAsync(u);
                var isValid = await _userManager.VerifyUserTokenAsync(user, "MagicLoginTokenProvider", "hood-login", t);
                if (!isValid)
                {
                    throw new UnauthorizedAccessException("The token " + t + " is not valid for the user " + u);
                }

                await _userManager.UpdateSecurityStampAsync(user);

                // mark account active as this is from an email, so no confirmation is required.
                user.Active = true;
                user.EmailConfirmed = true;
                user.LastLogOn = DateTime.UtcNow;
                user.LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString();
                user.LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString();

                await _userManager.UpdateAsync(user);

                await _signInManager.SignInAsync(user, true);

                return RedirectToAction(nameof(MagicLoginSuccess), new { returnUrl = r });
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("account/login/success")]
        public virtual IActionResult MagicLoginSuccess(LoginSuccessModel model)
        {
            if (!Url.IsLocalUrl(model.returnUrl))
            {
                model.returnUrl = "/";
            }
            return View(model);
        }

        #endregion

>>>>>>> dev
        #region Password Registration

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
                if (!recaptcha.Success)
                {
                    ModelState.AddModelError(string.Empty, "You have failed to pass the reCaptcha check. Please refresh your page and try again.");
                    return View(model);
                }

                ApplicationUser user = new ApplicationUser
                {
                    UserName = model.Username.IsSet() ? model.Username : model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DisplayName = model.DisplayName,
                    PhoneNumber = model.Phone,
                    JobTitle = model.JobTitle,
                    Anonymous = model.Anonymous,
                    CreatedOn = DateTime.UtcNow,
                    LastLogOn = DateTime.UtcNow,
                    LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString(),
                    LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString()
                };
                IdentityResult result = await _account.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (Engine.Settings.Account.RequireEmailConfirmation)
                    {
                        await _account.SendVerificationEmail(user);
                    }
                    else
                    {
                        await SendWelcomeEmail(user);
                    }

                    user.Active = !Engine.Settings.Account.RequireEmailConfirmation;
                    user.EmailConfirmed = !Engine.Settings.Account.RequireEmailConfirmation;
                    user.LastLogOn = DateTime.UtcNow;
                    user.LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString();
                    user.LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString();

                    await _account.UpdateUserAsync(user);

                    if (Engine.Auth0Enabled)
                    {
#warning Auth0 - Register - sign in user and redirect to return url - does this need a roundtrip to Auth0?
                        throw new NotImplementedException();
                    }
                    else
                    {
                        var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
                        await signInManager.SignInAsync(user, isPersistent: false);
                    }

                    if (Engine.Settings.Account.RequireEmailConfirmation)
                    {
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
            if (Engine.Auth0Enabled)
            {
                return RedirectToAction(nameof(SignOut), new { returnUrl });
            }
            else
            {
                System.Security.Principal.IIdentity user = User.Identity;
                var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
                await signInManager.SignOutAsync();
                await _logService.AddLogAsync<AccountController<TContext>>($"User ({user.Name}) logged out.");
                return RedirectToAction("Index", "Home");
            }
        }
        #endregion

        #region Auth0 
        [HttpGet]
        [AllowAnonymous]
        [Route("account/authorize")]
        public async Task Authorize(string returnUrl = "/")
        {
            if (!Engine.Auth0Enabled)
            {
<<<<<<< HEAD
                throw new ApplicationException("This endpoint is only available when using Auth0.");
            }
=======
                if (ModelState.IsValid)
                {

                    Services.RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
                    if (!recaptcha.Success)
                    {
                        ModelState.AddModelError(string.Empty, "You have failed to pass the reCaptcha check. Please refresh your page and try again.");
                        return View("MagicRegister", model);
                    }

                    if (!model.Consent)
                    {
                        ModelState.AddModelError(string.Empty, "You did not give consent for us to store your data, therefore we cannot complete the signup process.");
                    }


                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true

                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        // create the user
                        user = new ApplicationUser()
                        {
                            UserName = model.Username.IsSet() ? model.Username : model.Email,
                            Email = model.Email,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            DisplayName = model.DisplayName,
                            PhoneNumber = model.Phone,
                            JobTitle = model.JobTitle,
                            Anonymous = model.Anonymous,
                            CreatedOn = DateTime.UtcNow,
                            LastLogOn = DateTime.UtcNow,
                            LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString(),
                            LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString()
                        };
                        IdentityResult createUserResult = await _userManager.CreateAsync(user);
                        if (!createUserResult.Succeeded)
                        {
                            throw new Exception("Could not create new user.");
                        }
                    }

                    var token = await _userManager.GenerateUserTokenAsync(user, "MagicLoginTokenProvider", "hood-registration");

                    string callbackUrl = Url.Action("MagicRegister", "Account", new { u = user.Id, t = token, r = returnUrl }, protocol: HttpContext.Request.Scheme);

                    MailObject message = new MailObject()
                    {
                        To = new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                        PreHeader = $"Finish setting up account for {Engine.Settings.Basic.Title}",
                        Subject = $"Finish setting up account for {Engine.Settings.Basic.Title}"
                    };
                    message.AddParagraph($"Your account is ready, simply click the button below to confirm your email and log in.");
                    message.AddCallToAction("Finish & Login", callbackUrl);
                    message.Template = MailSettings.PlainTemplate;
                    await _emailSender.SendEmailAsync(message);

                    return RedirectToAction(nameof(MagicRegisterSent));
>>>>>>> dev

            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()

                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/auth/signout")]
        public async Task SignOut(string returnUrl = "/")
        {
            if (!Engine.Auth0Enabled)
            {
                throw new ApplicationException("This endpoint is only available when using Auth0.");
            }

            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be added to the
                // **Allowed Logout URLs** settings for the app.
                .WithRedirectUri(returnUrl.IsSet() ? returnUrl : Url.Action("Index", "Home"))
                .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/auth/failed")]
        public IActionResult RemoteSigninFailed(string r)
        {
            if (!Engine.Auth0Enabled)
            {
                throw new ApplicationException("This endpoint is only available when using Auth0.");
            }
            switch (r)
            {
                case "signup-disabled":
                    ViewData["Reason"] = "Sign up is not allowed at the moment.";
                    break;
                case "auth-failed":
                    ViewData["Reason"] = "Authentication failed, likely just a loose wire. Please try again.";
                    break;
                case "remote-failed":
                    ViewData["Reason"] = "We could not sign you in due to a techical issue, likely just a loose wire. Please try again.";
                    break;
            }
            return View();
        }

        [HttpGet]
        [Authorize(Hood.Identity.Policies.AccountNotConnected)]
        [Route("account/auth/connect-account")]
        public IActionResult ConnectAccount()
        {
            if (!Engine.Auth0Enabled)
            {
                throw new ApplicationException("This endpoint is only available when using Auth0.");
            }
            return View();
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
            if (User.Identity.IsAuthenticated && User.RequiresConnection())
            {
                return RedirectToAction(nameof(ConnectAccount));
            }
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
        [Route("account/confirm/required")]
        public virtual IActionResult ConfirmRequired(ConfirmRequiredModel model)
        {
            if (Engine.Auth0Enabled)
            {
                return View("ConfirmRequiredAuth0");
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [DisableForAuth0]
        [Route("account/confirm")]
        public virtual async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController<TContext>.Index), "Home");
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
                    RedirectToAction(nameof(ManageController.Index), "Manage");
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
        [AllowAnonymous]
        [DisableForAuth0]
        [Route("account/confirm/resend")]
        public virtual async Task<IActionResult> ResendConfirm()
        {
            ApplicationUser user = await GetCurrentUserOrThrow();
            await _account.SendVerificationEmail(user);
            return RedirectToAction(nameof(AccountController.ConfirmRequired), "Account");
        }

        #endregion

        #region Change Password
        [HttpGet]
        [Authorize(Hood.Identity.Policies.Active)]
        [Route("account/manage/change-password")]
        public virtual IActionResult ChangePassword()
        {
            if (Engine.Auth0Enabled)
            {
                return View("ChangePasswordAuth0");
            }
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [DisableForAuth0]
        [Authorize(Hood.Identity.Policies.Active)]
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

            MessageType = AlertType.Success;
            SaveMessage = "Your password has been changed.";

            return RedirectToAction(nameof(ChangePassword));
        }

        #endregion

        #region Forgot / Reset Password

        [HttpGet]
        [AllowAnonymous]
        [DisableForAuth0]
        [Route("account/forgot-password")]
        public virtual IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [DisableForAuth0]
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

                await _account.SendPasswordResetToken(user);

                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [DisableForAuth0]
        [Route("account/forgot-password/confirm")]
        public virtual IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [DisableForAuth0]
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
        [DisableForAuth0]
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
        [DisableForAuth0]
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
                ApplicationUser user = await GetCurrentUserOrThrow();
                await _account.DeleteUserAsync(user.Id, User);

                if (Engine.Auth0Enabled)
                {
                    return RedirectToAction(nameof(SignOut));
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
        #endregion

        #region Helpers

        protected async Task SendWelcomeEmail(ApplicationUser user)
        {
            string loginLink = Url.Action("Login", "Account", null, protocol: HttpContext.Request.Scheme);
            WelcomeEmailModel welcomeModel = new WelcomeEmailModel(user, loginLink)
            {
                SendToRecipient = true,
                NotifyRole = Engine.Settings.Account.NotifyNewAccount ? "NewAccountNotifications" : null
            };
            await _mailService.ProcessAndSend(welcomeModel);
        }

        protected void AddErrors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        protected IActionResult RedirectToLocal(string returnUrl)
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

        protected IActionResult RedirectWithReturnUrl(string url, string returnUrl)
        {
            if (returnUrl != null)
            {
                url += "&returnUrl=" + System.Net.WebUtility.UrlEncode(returnUrl);
            }

            return RedirectToLocal(url);
        }

        #endregion
    }
}