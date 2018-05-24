using Hood.Caching;
using Hood.Core;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    [Authorize]
    public abstract class BaseAccountController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly IEmailSender _emailSender;
        protected readonly IAccountRepository _account;
        protected readonly ISmsSender _smsSender;
        protected readonly ILogger _logger;
        protected readonly IContentRepository _data;
        protected readonly IHoodCache _cache;
        protected readonly ISettingsRepository _settings;

        public BaseAccountController(
            IContentRepository data,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            IHoodCache cache,
            ILoggerFactory loggerFactory,
            IAccountRepository account,
            ISettingsRepository settings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _account = account;
            _logger = loggerFactory.CreateLogger<BaseAccountController>();
            _data = data;
            _cache = cache;
            _settings = settings;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        #region "Login"

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult Login(string returnUrl = null)
        {

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    user.LastLogOn = DateTime.Now;
                    user.LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString();
                    user.LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString();
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation(1, "User " + model.Username + " logged in.");
                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning(2, "User account locked out.");
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

        [HttpGet]
        [AllowAnonymous]
        public virtual async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var model = new LoginWith2faViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return View();
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new ApplicationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return View();
            }
        }

        #endregion

        #region "Basic Registration"

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult Register(string returnUrl = null)
        {
            var accountSettings = _settings.GetAccountSettings();
            if (!accountSettings.EnableRegistration)
                return NotFound();
            if (accountSettings.RegistrationType == "code")
            {
                var url = "/account/create";
                if (returnUrl != null)
                    url += "?returnUrl=" + System.Net.WebUtility.UrlEncode(returnUrl);
                return Redirect(url);
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            var accountSettings = _settings.GetAccountSettings();
            if (!accountSettings.EnableRegistration)
                return NotFound();

            ViewData["ReturnUrl"] = returnUrl;

            if (!model.Consent)
            {
                ModelState.AddModelError(string.Empty, "You did not give consent for us to store your data, therefore we cannot complete the signup process");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username.IsSet() ? model.Username : model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                    WelcomeEmailSender welcomeSender = EngineContext.Current.Resolve<WelcomeEmailSender>();
                    await welcomeSender.ProcessAndSend(new WelcomeEmailModel(user, callbackUrl));

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region "Code Based Login"

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult Create(string returnUrl)
        {
            var accountSettings = _settings.GetAccountSettings();
            if (!accountSettings.EnableRegistration)
                return NotFound();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Create(RegisterCodeViewModel model, string returnUrl)
        {
            var accountSettings = _settings.GetAccountSettings();
            if (!accountSettings.EnableRegistration)
                return NotFound();

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var keyGen = new KeyGenerator(true, true, true, false);

                // check if the user is registered, if so forward to login, filling in the email address.
                var user = _account.GetUserByEmail(model.Email);
                if (user == null)
                {
                    // if no account exists, create one, then carry on.
                    user = new ApplicationUser { UserName = Guid.NewGuid().ToString(), Email = model.Email };
                    var result = await _userManager.CreateAsync(user, keyGen.Generate(24));
                    if (!result.Succeeded)
                    {
                        AddErrors(result);
                        return View(model);
                    }
                }

                // the user exists, has a code, but isn't set up for access, forward to the complete page
                if (user.EmailConfirmed)
                {
                    if (!user.Active)
                    {
                        return RedirectWithReturnUrl("/account/finish?uid=" + user.Id, returnUrl);
                    }
                    else
                    {
                        // They are good, let them log in.
                        return RedirectWithReturnUrl("/account/login?uid=", returnUrl);
                    }
                }

                // check if they have a current valid code, if so forward them to the code page.
                user = _account.GetUserById(user.Id);
                if (CheckForAccessCodes(user))
                {
                    return RedirectWithReturnUrl("/account/code?uid=" + user.Id, returnUrl);
                }

                // If they don't have a valid code, let them add a new code to their account, send it and forward to the code page.

                // Attach the code to the user, along with it's expiry date.
                keyGen = new KeyGenerator(false, false, true, false);
                var code = keyGen.Generate(6);
                if (user.AccessCodes == null)
                    user.AccessCodes = new List<UserAccessCode>();
                user.AccessCodes.Add(
                    new UserAccessCode()
                    {
                        Code = code,
                        Expiry = DateTime.Now.AddHours(accountSettings.CodeExpiry),
                        UserId = user.Id,
                        Type = "Registration"
                    }
                );
                await _userManager.UpdateAsync(user);

                MailObject message = new MailObject()
                {
                    To = new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                    PreHeader = _settings.ReplacePlaceholders("Your access code."),
                    Subject = _settings.ReplacePlaceholders("Your access code.")
                };
                message.AddH1(_settings.ReplacePlaceholders("Your access code."));
                message.AddParagraph($"You can use the following access code to complete your registration:");
                message.AddH2(code.Substring(0, 3) + " - " + code.Substring(3, 3), "#5fba7d", "center");
                message.AddParagraph($"Once you have entered your code you can create a username and password for the site. Click the button below to enter your code now.");
                message.AddCallToAction("Validate your account", "/account/code?uid=" + user.Id);
                await _emailSender.SendEmailAsync(message, MailSettings.SuccessTemplate);

                return RedirectWithReturnUrl("/account/code?uid=" + user.Id, returnUrl);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult Code(string uid, string returnUrl)
        {
            var accountSettings = _settings.GetAccountSettings();
            if (!accountSettings.EnableRegistration)
                return NotFound();

            // Here we must flag the account as email confirmed. If the code entered matches. 

            // If the userid is not set then we cannot validate them! 
            // Back to user create we go.
            if (!uid.IsSet())
            {
                return RedirectWithReturnUrl("/account/create", returnUrl);
            }

            // check if they have a current valid code, if not forward them back to the code page forward them to the code page.
            var user = _account.GetUserById(uid);
            if (!CheckForAccessCodes(user))
            {
                // generate one with their account info, and re-display the code page.
                return RedirectWithReturnUrl("/account/create", returnUrl);
            }

            // First off, let's display the input for the code, let's go ahead and display the form.
            return View(new EnterCodeViewModel() { UserId = uid });
        }

        [HttpPost]
        [AllowAnonymous]
        public virtual IActionResult Code(EnterCodeViewModel model, string returnUrl)
        {
            var accountSettings = _settings.GetAccountSettings();
            if (!accountSettings.EnableRegistration)
                return NotFound();

            // check if they have a current valid code, if not forward them back to the create page to set up a new code.
            var user = _account.GetUserById(model.UserId);
            if (!CheckForAccessCodes(user))
            {
                return RedirectWithReturnUrl("/account/code?uid=" + user.Id, returnUrl);
            }

            var codes = user.AccessCodes.Where(ac =>
                ac.Type == "Registration" &&
                ac.Expiry > DateTime.Now &&
                !ac.Used);

            // Here we must flag the account as email confirmed. If the code entered matches. 
            var code = model.Code.Replace("-", "");

            foreach (var test in codes)
            {
                if (code == test.Code)
                {
                    // We found a matching code. Update the user as confirmed, and then redirect on to the complete setup page.
                    user.EmailConfirmed = true;
                    _account.UpdateUser(user);
                    return RedirectWithReturnUrl("/account/finish?uid=" + user.Id, returnUrl);
                }
            }

            ModelState.AddModelError("Invalid Code", "The code you have entered is not valid.");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult Finish(string uid, string returnUrl)
        {
            var accountSettings = _settings.GetAccountSettings();
            if (!accountSettings.EnableRegistration)
                return NotFound();

            // Here we must flag the account as email confirmed. If the code entered matches. 

            // If the userid is not set then we cannot validate them! 
            // Back to user create we go.
            if (!uid.IsSet())
            {
                return RedirectWithReturnUrl("/account/create", returnUrl);
            }

            // check if they have a current valid code, if not forward them back to the code page forward them to the code page.
            var user = _account.GetUserById(uid);
            if (!user.EmailConfirmed)
            {
                // generate one with their account info, and re-display the code page.
                return RedirectWithReturnUrl("/account/create", returnUrl);
            }

            // First off, let's display the input for the code, let's go ahead and display the form.
            return View(new FinishAccountSetupModel() { UserId = uid });
        }

        [HttpPost]
        [AllowAnonymous]
        public virtual async Task<IActionResult> Finish(FinishAccountSetupModel model, string returnUrl)
        {
            try
            {
                var accountSettings = _settings.GetAccountSettings();
                if (!accountSettings.EnableRegistration)
                    return NotFound();

                // check if they have a current valid code, if not forward them back to the code page forward them to the code page.
                var user = _account.GetUserById(model.UserId);
                if (!user.EmailConfirmed)
                {
                    // generate one with their account info, and re-display the code page.
                    return RedirectWithReturnUrl("/account/create", returnUrl);
                }

                user.UserName = model.Username;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                _account.UpdateUser(user);

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(3, "User created a new account with password.");

                WelcomeEmailSender welcomeSender = EngineContext.Current.Resolve<WelcomeEmailSender>();
                await welcomeSender.ProcessAndSend(new WelcomeEmailModel(user));

                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(ex.Message, ex.Message);
                return View(model);
            }
        }

        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(4, "User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        public virtual IActionResult AccessDenied()
        {
            return View();
        }

        #region "External Logins"

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    throw new ApplicationException("Error loading external login information during confirmation.");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }        

        #endregion

        [HttpGet]
        [AllowAnonymous]
        public virtual async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(BaseHomeController.Index), "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                // Send an email with this link
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                MailObject message = new MailObject()
                {
                    To = new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                    PreHeader = _settings.ReplacePlaceholders("Reset your password."),
                    Subject = _settings.ReplacePlaceholders("Reset your password.")
                };
                message.AddH1(_settings.ReplacePlaceholders("Reset your password."));
                message.AddParagraph($"Please reset your password by clicking here:");
                message.AddCallToAction("Reset your password", callbackUrl);
                await _emailSender.SendEmailAsync(message, MailSettings.WarningTemplate);
                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public virtual IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                return View("Error");
            }
            if (model.SelectedProvider == "Email")
            {
                MailObject message = new MailObject()
                {
                    To = new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                    PreHeader = "Your security code for accessing the website...",
                    Subject = "Your security code..."
                };
                message.AddParagraph("Your security code is: " + code);
                await _emailSender.SendEmailAsync(message);
            }
            else if (model.SelectedProvider == "Phone")
            {
                await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), "Your security code is: " + code);
            }

            return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return View(model);
            }
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        private IActionResult RedirectToLocal(string returnUrl)
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

        private IActionResult RedirectWithReturnUrl(string url, string returnUrl)
        {
            if (returnUrl != null)
                url += "&returnUrl=" + System.Net.WebUtility.UrlEncode(returnUrl);
            return RedirectToLocal(url);
        }

        private bool CheckForAccessCodes(ApplicationUser user)
        {
            if (user.AccessCodes == null)
                return false;

            var codes = user.AccessCodes.Where(ac =>
                    ac.Type == "Registration" &&
                    ac.Expiry > DateTime.Now &&
                    !ac.Used);

            if (codes.Count() > 0)
                return true;

            return false;
        }

        #endregion
    }
}