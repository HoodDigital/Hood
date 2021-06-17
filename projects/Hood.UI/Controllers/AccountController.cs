using Hood.Core;
using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
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
    [Authorize]
    public abstract class AccountController : AccountController<HoodDbContext>
    {
        public AccountController() : base() { }
    }

    [Authorize]
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
            ViewData["ReturnUrl"] = returnUrl;

            AccountSettings accountSettings = Engine.Settings.Account;
            if (accountSettings.MagicLinkLogin)
            {
                return View("MagicLogin");
            }

            return View("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/login/password")]
        public virtual IActionResult LoginPassword(string returnUrl = null)
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

            if (ModelState.IsValid && ModelState.IsNotSpam(model))
            {

                Services.RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
                if (!recaptcha.Success)
                {
                    ModelState.AddModelError(string.Empty, "You have failed to pass the reCaptcha check.");
                    return View(model);
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    ApplicationUser user = await _userManager.FindByNameAsync(model.Username);
                    user.LastLogOn = DateTime.UtcNow;
                    user.LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString();
                    user.LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString();
                    await _userManager.UpdateAsync(user);

                    await _logService.AddLogAsync<AccountController<TContext>>($"User ({model.Username}) logged in.");

                    if (Engine.Settings.Account.RequireEmailConfirmation && !user.EmailConfirmed)
                    {
                        await SendWelcomeEmail(user, true);
                        return RedirectToAction(nameof(ConfirmRequired), new { user = user.Id });
                    }

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
                if (ModelState.IsValid && ModelState.IsNotSpam(model))
                {

                    Services.RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
                    if (!recaptcha.Success)
                    {
                        ModelState.AddModelError(string.Empty, "You have failed to pass the reCaptcha check.");
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

                // mark account active etc etc.
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

        #region Password Registration

        [HttpGet]
        [AllowAnonymous]
        [Route("account/register")]
        public virtual IActionResult Register(string returnUrl = null)
        {
            AccountSettings accountSettings = Engine.Settings.Account;
            if (!accountSettings.EnableRegistration)
            {
                return RegistrationClosed();
            }
            if (accountSettings.MagicLinkLogin)
            {
                return View("MagicRegister");
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
            if (accountSettings.MagicLinkLogin)
            {
                return RedirectToAction(nameof(Register));
            }

            ViewData["ReturnUrl"] = returnUrl;

            if (!model.Consent)
            {
                ModelState.AddModelError(string.Empty, "You did not give consent for us to store your data, therefore we cannot complete the signup process.");
            }

            if (ModelState.IsValid && ModelState.IsNotSpam(model))
            {

                Services.RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
                if (!recaptcha.Success)
                {
                    ModelState.AddModelError(string.Empty, "You have failed to pass the reCaptcha check.");
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
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SendWelcomeEmail(user, Engine.Settings.Account.RequireEmailConfirmation);

                    user.Active = !Engine.Settings.Account.RequireEmailConfirmation;
                    user.LastLogOn = DateTime.UtcNow;
                    user.LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString();
                    user.LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString();

                    await _userManager.UpdateAsync(user);

                    await _signInManager.SignInAsync(user, isPersistent: false);

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

        #region Magic Link Registration

        [HttpGet]
        [AllowAnonymous]
        [Route("account/register/generate")]
        public virtual IActionResult MagicRegisterGenerate()
        {
            return RedirectToAction(nameof(Register));
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("account/register/generate")]
        public virtual async Task<IActionResult> MagicRegisterGenerate(MagicRegisterViewModel model, string returnUrl = null)
        {
            AccountSettings accountSettings = Engine.Settings.Account;
            if (!accountSettings.EnableRegistration)
            {
                return RedirectToAction(nameof(RegistrationClosed));
            }
            if (!accountSettings.MagicLinkLogin)
            {
                return RedirectToAction(nameof(Register));
            }

            ViewData["ReturnUrl"] = returnUrl;
            try
            {
                if (ModelState.IsValid && ModelState.IsNotSpam(model))
                {

                    Services.RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
                    if (!recaptcha.Success)
                    {
                        ModelState.AddModelError(string.Empty, "You have failed to pass the reCaptcha check.");
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

                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            // If we got this far, something failed, redisplay form
            return View("MagicRegister", model);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/register/sent")]
        public virtual IActionResult MagicRegisterSent(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [AllowAnonymous]
        [Route("account/register/code")]
        public virtual async Task<IActionResult> MagicRegister(string t, string u, string r = "/")
        {
            try
            {
                var user = await _userManager.FindByIdAsync(u);
                var isValid = await _userManager.VerifyUserTokenAsync(user, "MagicLoginTokenProvider", "hood-registration", t);
                if (!isValid)
                {
                    throw new UnauthorizedAccessException("The token " + t + " is not valid for the user " + u);
                }
                await _userManager.UpdateSecurityStampAsync(user);

                // send welcome email, email already confirmed so no confirm email link.
                await SendWelcomeEmail(user, false);

                // mark account active etc etc.
                user.Active = true;
                user.EmailConfirmed = true;
                user.LastLogOn = DateTime.UtcNow;
                user.LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString();
                user.LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString();

                await _userManager.UpdateAsync(user);

                await _signInManager.SignInAsync(user, true);

                return RedirectToAction(nameof(MagicRegisterSuccess), new { returnUrl = r });
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException(ex.Message);
            }
        }

        [AllowAnonymous]
        [Route("account/register/success")]
        public virtual IActionResult MagicRegisterSuccess(RegisterSuccessModel model)
        {
            if (!Url.IsLocalUrl(model.returnUrl))
            {
                model.returnUrl = "/";
            }
            return View(model);
        }

        #endregion

        #region Log Off

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("account/logout")]
        public virtual async Task<IActionResult> LogOff()
        {
            System.Security.Principal.IIdentity user = User.Identity;
            await _signInManager.SignOutAsync();
            await _logService.AddLogAsync<AccountController<TContext>>($"User ({user.Name}) logged out.");
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
        [Route("account/access-denied")]
        public virtual IActionResult AccessDenied()
        {
            return View();
        }

        #endregion

        #region Confirm Email

        [HttpGet]
        [AllowAnonymous]
        [Route("account/confirm/required")]
        public virtual IActionResult ConfirmRequired(ConfirmRequiredModel model)
        {
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/confirm")]
        public virtual async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController<TContext>.Index), "Home");
            }
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);
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
                await _userManager.UpdateAsync(user);
            }
            return View("ConfirmEmail");
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/confirm/resend")]
        public virtual async Task<IActionResult> ResendConfirm(ConfirmRequiredModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{model.UserId}'.");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: HttpContext.Request.Scheme);
            var verifyModel = new VerifyEmailModel(user, callbackUrl)
            {
                SendToRecipient = true
            };

            await _mailService.ProcessAndSend(verifyModel);

            return RedirectToAction(nameof(AccountController.ConfirmRequired), "Account");
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
                ApplicationUser user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                string code = await _userManager.GeneratePasswordResetTokenAsync(user);
                string callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code }, protocol: HttpContext.Request.Scheme);

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
            ApplicationUser user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            IdentityResult result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
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

        #region Login Page Backgrounds 

        [HttpGet]
        [AllowAnonymous]
        [Route("hood/images/random/{query?}")]
        public virtual async Task<IActionResult> BackgroundImage(string query)
        {
            if (Engine.Settings.Integrations.UnsplashAccessKey.IsSet())
            {
                var client = new UnsplasharpClient(Engine.Settings.Integrations.UnsplashAccessKey);
                var photosFound = await client.GetRandomPhoto(UnsplasharpClient.Orientation.Squarish, query: query);
                return Json(photosFound);
            }
            else
            {
                try
                {
                    return Content(Engine.Settings.Basic.LoginAreaSettings.BackgroundImages.Split(Environment.NewLine).PickRandom());
                }
                catch
                {
                    return Content("https://source.unsplash.com/random");
                }
            }
        }

        #endregion

        #region Helpers

        protected async Task SendWelcomeEmail(ApplicationUser user, bool confirmEmail)
        {
            if ((Engine.Settings.Account.EnableWelcome || Engine.Settings.Account.RequireEmailConfirmation) && Engine.Settings.Mail.IsSetup)
            {
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                string callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: HttpContext.Request.Scheme);
                WelcomeEmailModel welcomeModel = new WelcomeEmailModel(user, confirmEmail ? callbackUrl : null)
                {
                    SendToRecipient = true,
                    NotifyRole = Engine.Settings.Account.NotifyNewAccount ? "NewAccountNotifications" : null
                };
                await _mailService.ProcessAndSend(welcomeModel);
            }
        }

        protected void AddErrors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        protected Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
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