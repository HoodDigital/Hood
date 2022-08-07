// using Auth0.AspNetCore.Authentication;
// using Auth0.AuthenticationApi;
// using Auth0.AuthenticationApi.Models;
// using Auth0.Core.Exceptions;
// using Hood.Attributes;
// using Hood.BaseTypes;
// using Hood.Core;
// using Hood.Enums;
// using Hood.Extensions;
// using Hood.Identity;
// using Hood.Interfaces;
// using Hood.Models;
// using Hood.Services;
// using Hood.ViewModels;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Rendering;
// using Microsoft.AspNetCore.Routing;
// using Microsoft.EntityFrameworkCore;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using Unsplasharp;

// namespace Hood.BaseControllers
// {
//     public abstract class AccountController : BaseController
//     {
//         protected readonly UserManager<ApplicationUser> _userManager;
//         protected readonly IPasswordAccountRepository _account;
//         public AccountController()
//         {
//             _userManager = Engine.Services.Resolve<UserManager<ApplicationUser>>();
//             _account = Engine.Services.Resolve<IPasswordAccountRepository>();
//         }

//         #region Account Home

//         [HttpGet]
//         [Route("account/")]
//         public virtual async Task<IActionResult> Index(string returnUrl, bool created = false)
//         {
//             IUserProfile user = await GetCurrentUserOrThrow();
//             var model = new UserViewModel
//             {
//                 LocalUserId = user.Id,
//                 Username = user.UserName,
//                 Email = user.Email,
//                 PhoneNumber = user.PhoneNumber,
//                 IsEmailConfirmed = user.EmailConfirmed,
//                 StatusMessage = SaveMessage,
//                 Avatar = user.Avatar,
//                 Profile = await _account.GetUserProfileByIdAsync(user.Id),
//                 Accounts = user.ConnectedAuth0Accounts,
//                 ReturnUrl = returnUrl,
//                 NewAccountCreated = created
//             };
//             return View(model);
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         [Route("account/")]
//         public virtual async Task<IActionResult> Index(UserViewModel model, string returnUrl, bool created = false)
//         {
//             var user = await _account.GetUserByIdAsync(User.GetLocalUserId());
//             try
//             {
//                 if (!ModelState.IsValid)
//                 {
//                     return View(model);
//                 }

//                 var email = user.Email;
//                 if (model.Email != email)
//                 {
//                     await _account.SetEmailAsync(user, model.Email);
//                 }

//                 var phoneNumber = user.PhoneNumber;
//                 if (model.PhoneNumber != phoneNumber)
//                 {
//                     await _account.SetPhoneNumberAsync(user, model.PhoneNumber);
//                 }

//                 await _account.UpdateProfileAsync(model.Profile);

//                 user = await GetCurrentUserOrThrow();

//                 User.SetUserClaims(user);
//                 if (Engine.Auth0Enabled)
//                 {
//                     await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);
//                 }
//                 else
//                 {
//                     var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
//                     await signInManager.SignInAsync(user, isPersistent: false);
//                 }

//                 SaveMessage = "Your profile has been updated.";
//                 MessageType = AlertType.Success;
//                 return RedirectToAction(nameof(Index));

//             }
//             catch (Exception ex)
//             {
//                 SaveMessage = "Something went wrong: " + ex.Message;
//                 MessageType = AlertType.Danger;
//                 model.Accounts = user.ConnectedAuth0Accounts;
//                 model.ReturnUrl = returnUrl;
//                 model.NewAccountCreated = created;
//                 return View(model);
//             }

//         }

//         [Route("account/avatar")]
//         public virtual async Task<Response> UploadAvatar(IFormFile file, string userId)
//         {
//             // User must have an organisation.
//             try
//             {

//                 IUserProfile user = await GetCurrentUserOrThrow();
//                 IMediaObject mediaResult = null;
//                 if (file != null)
//                 {
//                     // If the club already has an avatar, delete it from the system.
//                     if (user.Avatar != null)
//                     {
//                         var mediaItem = await _db.Media.SingleOrDefaultAsync(m => m.UniqueId == user.Avatar.UniqueId);
//                         if (mediaItem != null)
//                             _db.Entry(mediaItem).State = EntityState.Deleted;
//                         await _media.DeleteStoredMedia((MediaObject)user.Avatar);
//                     }
//                     var directory = await _account.GetDirectoryAsync(User.GetLocalUserId());
//                     mediaResult = await _media.ProcessUpload(file, _directoryManager.GetPath(directory.Id));
//                     user.Avatar = mediaResult;
//                     await _account.UpdateUserAsync(user);
//                     _db.Media.Add(new MediaObject(mediaResult, directory.Id));
//                     await _db.SaveChangesAsync();

//                     User.SetUserClaims(user);
//                     await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);
//                 }
//                 return new Response(true, mediaResult, $"The media has been set for attached successfully.");
//             }
//             catch (Exception ex)
//             {
//                 return await ErrorResponseAsync<AccountController>($"There was an error setting the avatar.", ex);
//             }
//         }

//         #endregion

//         #region Login - Password 

//         [HttpGet]
//         [AllowAnonymous]
//         [Route("account/login")]
//         public virtual IActionResult Login(string returnUrl = null)
//         {
//             if (Engine.Auth0Enabled)
//             {
//                 // redirect the user to the sign up endpoint... somehow.
//                 return RedirectToAction(nameof(Authorize), new { returnUrl, mode = Engine.Settings.Account.MagicLinkLogin ? "passwordless" : "login" });
//             }
//             else
//             {
//                 ViewData["ReturnUrl"] = returnUrl;
//                 return View("Login");
//             }
//         }

//         [HttpPost]
//         [AllowAnonymous]
//         [DisableForAuth0]
//         [ValidateAntiForgeryToken]
//         [Route("account/login")]
//         public virtual async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
//         {
//             AccountSettings accountSettings = Engine.Settings.Account;
//             ViewData["ReturnUrl"] = returnUrl;

//             if (ModelState.IsValid)
//             {

//                 Services.RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
//                 if (!recaptcha.Passed)
//                 {
//                     ModelState.AddModelError(string.Empty, "You have failed to pass the reCaptcha check. Please refresh your page and try again.");
//                     return View(model);
//                 }

//                 // This doesn't count login failures towards account lockout
//                 // To enable password failures to trigger account lockout, set lockoutOnFailure: true                
//                 var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
//                 Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, lockoutOnFailure: false);
//                 if (result.Succeeded)
//                 {
//                     ApplicationUser user = await _userManager.FindByNameAsync(model.Username);
//                     if (Engine.Settings.Account.RequireEmailConfirmation && !user.EmailConfirmed)
//                     {
//                         await SendVerificationEmail(user, User.GetUserId(), Url.AbsoluteAction("Login", "Account"));
//                         return RedirectToAction(nameof(ConfirmRequired), new { user = user.Id });
//                     }

//                     user.LastLogOn = DateTime.UtcNow;
//                     user.LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString();
//                     user.LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString();
//                     await _account.UpdateUserAsync(user);

//                     await _logService.AddLogAsync<AccountController<TContext>>($"User ({model.Username}) logged in.");

//                     return RedirectToLocal(returnUrl);
//                 }

//                 if (result.IsLockedOut)
//                 {
//                     return View("Lockout");
//                 }
//                 else
//                 {
//                     ModelState.AddModelError(string.Empty, "Invalid login attempt.");
//                     return View(model);
//                 }
//             }

//             // If we got this far, something failed, redisplay form
//             return View(model);
//         }

//         #endregion

//         #region Registration - Password

//         [HttpGet]
//         [AllowAnonymous]
//         [Route("account/register")]
//         public virtual IActionResult Register(string returnUrl = null)
//         {
//             AccountSettings accountSettings = Engine.Settings.Account;
//             if (!accountSettings.EnableRegistration)
//             {
//                 return RedirectToAction(nameof(RegistrationClosed));
//             }
//             if (Engine.Auth0Enabled)
//             {
//                 // redirect the user to the sign up endpoint... somehow.
//                 return RedirectToAction(nameof(Authorize), new { returnUrl, mode = Engine.Settings.Account.MagicLinkLogin ? "passwordless" : "signup" });
//             }
//             return View();
//         }

//         [HttpPost]
//         [AllowAnonymous]
//         [DisableForAuth0]
//         [ValidateAntiForgeryToken]
//         [Route("account/register")]
//         public virtual async Task<IActionResult> Register(PasswordRegisterViewModel model, string returnUrl = null)
//         {
//             AccountSettings accountSettings = Engine.Settings.Account;
//             if (!accountSettings.EnableRegistration)
//             {
//                 return RedirectToAction(nameof(RegistrationClosed));
//             }

//             ViewData["ReturnUrl"] = returnUrl;

//             if (!model.Consent)
//             {
//                 ModelState.AddModelError(string.Empty, "You did not give consent for us to store your data, therefore we cannot complete the signup process.");
//             }

//             if (ModelState.IsValid)
//             {

//                 Services.RecaptchaResponse recaptcha = await _recaptcha.Validate(Request);
//                 if (!recaptcha.Passed)
//                 {
//                     ModelState.AddModelError(string.Empty, "You have failed to pass the reCaptcha check. Please refresh your page and try again.");
//                     return View(model);
//                 }

//                 ApplicationUser user = new ApplicationUser
//                 {
//                     UserName = model.Username.IsSet() ? model.Username : model.Email,
//                     Email = model.Email,
//                     FirstName = model.FirstName,
//                     LastName = model.LastName,
//                     DisplayName = model.DisplayName,
//                     PhoneNumber = model.Phone,
//                     JobTitle = model.JobTitle,
//                     Anonymous = model.Anonymous,
//                     CreatedOn = DateTime.UtcNow,
//                     LastLogOn = DateTime.UtcNow,
//                     LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString(),
//                     LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString()
//                 };
//                 IdentityResult result = await _account.CreateAsync(user, model.Password);
//                 if (result.Succeeded)
//                 {
//                     await SendWelcomeEmail(user);

//                     user.Active = !Engine.Settings.Account.RequireEmailConfirmation;
//                     user.EmailConfirmed = !Engine.Settings.Account.RequireEmailConfirmation;
//                     user.LastLogOn = DateTime.UtcNow;
//                     user.LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString();
//                     user.LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString();

//                     await _account.UpdateUserAsync(user);

//                     var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
//                     await signInManager.SignInAsync(user, isPersistent: false);


//                     if (Engine.Settings.Account.RequireEmailConfirmation)
//                     {
//                         await SendVerificationEmail(user, User.GetUserId(), Url.AbsoluteAction("Login", "Account"));
//                         return RedirectToAction(nameof(AccountController.ConfirmRequired), "Account");
//                     }
//                     return RedirectToLocal(returnUrl);
//                 }
//                 AddErrors(result);
//             }

//             // If we got this far, something failed, redisplay form
//             return View(model);
//         }
//         #endregion

//         #region Logout
//         [HttpPost]
//         [Authorize]
//         [ValidateAntiForgeryToken]
//         [Route("account/logout")]
//         public virtual async Task<IActionResult> LogOff(string returnUrl = "/")
//         {
//             if (Engine.Auth0Enabled)
//             {
//                 return RedirectToAction(nameof(SignOut), new { returnUrl });
//             }
//             else
//             {
//                 System.Security.Principal.IIdentity user = User.Identity;
//                 var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
//                 await signInManager.SignOutAsync();
//                 await _logService.AddLogAsync<AccountController<TContext>>($"User ({user.Name}) logged out.");
//                 return RedirectToAction("Index", "Home");
//             }
//         }
//         #endregion

//         #region Auth0 - Sign in/out
//         [HttpGet]
//         [AllowAnonymous]
//         [Route("account/authorize")]
//         public virtual async Task Authorize(string returnUrl = "/", string mode = "login")
//         {
//             if (!Engine.Auth0Enabled)
//             {
//                 throw new ApplicationException("This endpoint is only available when using Auth0.");
//             }

//             // TODO: #269 Insert colours and logos and parameters for standard login.
//             var authenticationPropertiesBuilder = new LoginAuthenticationPropertiesBuilder()
//                 // .WithParameter("logo", "https://cdn.jsdelivr.net/npm/hoodcms@5.0.15/images/icons/file.png")
//                 // .WithParameter("color", "black")
//                 // .WithParameter("background", "orange")
//                 .WithRedirectUri(returnUrl);

//             authenticationPropertiesBuilder = authenticationPropertiesBuilder.WithParameter("action", mode);

//             var authenticationProperties = authenticationPropertiesBuilder.Build();

//             await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
//         }

//         [HttpGet]
//         [AllowAnonymous]
//         [Route("account/auth/signout")]
//         public virtual async Task SignOut(string returnUrl = "/")
//         {
//             if (!Engine.Auth0Enabled)
//             {
//                 throw new ApplicationException("This endpoint is only available when using Auth0.");
//             }

//             var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
//                 // Indicate here where Auth0 should redirect the user after a logout.
//                 // Note that the resulting absolute Uri must be added to the
//                 // **Allowed Logout URLs** settings for the app.
//                 .WithRedirectUri(returnUrl.IsSet() ? returnUrl : Url.Action("Index", "Home"))
//                 .Build();

//             await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
//             await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//         }

//         [HttpGet]
//         [AllowAnonymous]
//         [Route("account/auth/failed")]
//         public virtual IActionResult RemoteSigninFailed(string r, string d)
//         {
//             if (!Engine.Auth0Enabled)
//             {
//                 return NotFound();
//             }
//             switch (r)
//             {
//                 case "unauthorized":
//                     if (d.IsSet())
//                     {
//                         ViewData["Reason"] = "Unauthorized: " + d.ToTitleCase();
//                     }
//                     else
//                     {
//                         ViewData["Reason"] = "This account is not authorized.";
//                     }
//                     break;
//                 case "auth-failed":
//                     ViewData["allow-relog"] = true;
//                     ViewData["Reason"] = "Authentication failed, likely just a loose wire.";
//                     break;
//                 case "state-failure":
//                     ViewData["allow-relog"] = true;
//                     ViewData["Reason"] = "Authentication failed, looks like you may be trying to log in with a different browser or device. Make sure you are using the same device to click your login link.";
//                     break;
//                 case "remote-failed":
//                     ViewData["allow-relog"] = true;
//                     ViewData["Reason"] = "We could not sign you in due to a techical issue, likely just a loose wire.";
//                     break;
//                 case "account-creation-failed":
//                     ViewData["allow-relog"] = true;
//                     ViewData["Reason"] = "We could not create a local account due to a techical issue, likely just a loose wire.";
//                     break;
//                 case "account-linking-failed":
//                     ViewData["allow-relog"] = true;
//                     ViewData["Reason"] = "We could not connect your login to a local account due to a techical issue, likely just a loose wire.";
//                     break;
//             }
//             return View();
//         }
//         #endregion

//         #region Auth0 - Connect Account
//         [HttpGet]
//         [Authorize(Policies.AccountNotConnected)]
//         [Route("account/auth/connect")]
//         public virtual async Task<IActionResult> ConnectAccount(string returnUrl)
//         {
//             if (!Engine.Auth0Enabled)
//             {
//                 return NotFound();
//             }
//             var user = await GetCurrentUserOrThrow();
//             var model = new ConnectAccountModel()
//             {
//                 LocalPicture = user.GetAvatar(),
//                 ReturnUrl = returnUrl,
//                 RemotePicture = User.GetClaimValue(HoodClaimTypes.RemotePicture)
//             };
//             if (User.HasClaim(HoodClaimTypes.AccountLinkRequired))
//             {
//                 return View("ConnectAccountLink", model);
//             }
//             return View(model);
//         }
//         [HttpPost]
//         [Authorize(Policies.AccountNotConnected)]
//         [Route("account/auth/connect-confirm")]
//         public virtual async Task<IActionResult> ConnectAccountConfirm(string returnUrl)
//         {
//             if (!Engine.Auth0Enabled)
//             {
//                 return NotFound();
//             }
//             try
//             {
//                 if (User.HasClaim(HoodClaimTypes.AccountLinkRequired))
//                 {
//                     return RedirectToAction(nameof(ConnectAccount), new { returnUrl });
//                 }

//                 if (!User.IsEmailConfirmed())
//                 {
//                     // tell the user they need to confirm their email, then try again. Restart flow.
//                     return RedirectToAction(nameof(ConfirmRequired));
//                 }

//                 // connect the accounts.       
//                 var authService = new Auth0Service();

//                 var user = await GetCurrentUserOrThrow();
//                 var linkedUser = await authService.GetUserByAuth0UserId(User.GetUserId());
//                 if (linkedUser != null)
//                 {
//                     throw new Exception("This account is already connected to an account.");
//                 }

//                 var newAuthUser = await authService.CreateLocalAuthIdentity(User.GetUserId(), user, User.GetClaimValue(HoodClaimTypes.RemotePicture));
//                 if (newAuthUser == null)
//                 {
//                     throw new Exception("There was a problem connecting your account, please try again.");
//                 }

//                 // Update local account profile from remote account info - if set and not set etc etc.
//                 if (user.UpdateFromPrincipal(User))
//                 {
//                     await _account.UpdateUserAsync(user);
//                 }

//                 User.RemoveClaim(HoodClaimTypes.AccountNotConnected);
//                 User.AddOrUpdateClaimValue(HoodClaimTypes.Active, "true");
//                 await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);

//                 return RedirectToAction(nameof(ConnectAccountComplete), new { returnUrl });
//             }
//             catch (Exception ex)
//             {
//                 SaveMessage = "Could not connect the account: " + ex.Message;
//                 MessageType = AlertType.Danger;
//                 return RedirectToAction(nameof(ConnectAccount));
//             }
//         }
//         [HttpPost]

//         [Authorize(Policies.AccountNotConnected)]
//         [Route("account/auth/connect-link")]
//         public virtual async Task<IActionResult> ConnectAccountLink(string returnUrl)
//         {
//             if (!Engine.Auth0Enabled)
//             {
//                 return NotFound();
//             }
//             try
//             {
//                 var authService = new Auth0Service();
//                 if (!User.HasClaim(HoodClaimTypes.AccountLinkRequired))
//                 {
//                     return RedirectToAction(nameof(ConnectAccount), new { returnUrl });
//                 }

//                 if (!User.IsEmailConfirmed())
//                 {
//                     // tell the user they need to confirm their email, then try again. Restart flow.
//                     return RedirectToAction(nameof(ConfirmRequired));
//                 }

//                 // connect the accounts.             
//                 var user = await GetCurrentUserOrThrow();
//                 var linkedUser = await authService.GetUserByAuth0UserId(User.GetUserId());
//                 if (linkedUser != null)
//                 {
//                     throw new Exception("This account is already connected to an account.");
//                 }

//                 // get the user's other account....(s?)
//                 var primaryAccount = user.GetPrimaryIdentity();
//                 if (primaryAccount == null)
//                 {
//                     throw new Exception("Could not load a primary account to link.");
//                 }

//                 try
//                 {
//                     var fullAuthUserId = User.GetUserId();
//                     var authProviderName = fullAuthUserId.Split('|')[0];
//                     var authUserId = fullAuthUserId.Split('|')[1];
//                     var client = await authService.GetClientAsync();
//                     var response = await client.Users.LinkAccountAsync(primaryAccount.Id, new Auth0.ManagementApi.Models.UserAccountLinkRequest()
//                     {
//                         Provider = authProviderName,
//                         UserId = authUserId
//                     });

//                     // Success - remove link claim.
//                     User.RemoveClaim(HoodClaimTypes.AccountLinkRequired);
//                 }
//                 catch (ErrorApiException ex)
//                 {
//                     if (ex.ApiError == null)
//                     {
//                         throw new Exception("Could not link the remote accounts: " + ex.Message);
//                     }
//                     switch (ex.ApiError.ErrorCode)
//                     {
//                         case "identity_conflict":
//                             if (ex.ApiError.ExtraData.ContainsKey("statusCode") && ex.ApiError.ExtraData["statusCode"] == "409")
//                             {
//                                 // Account already linked, remove link claim and continue.
//                                 User.RemoveClaim(HoodClaimTypes.AccountLinkRequired);
//                             }
//                             break;
//                         default:
//                             throw new Exception("Could not link the remote accounts: " + ex.Message);
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     throw new Exception("Could not link the remote accounts: " + ex.Message);
//                 }

//                 var newAuthUser = await authService.CreateLocalAuthIdentity(User.GetUserId(), user, User.GetClaimValue(HoodClaimTypes.RemotePicture));
//                 if (newAuthUser == null)
//                 {
//                     throw new Exception("There was a problem connecting your account, please try again.");
//                 }

//                 User.AddOrUpdateClaimValue(HoodClaimTypes.Active, "true");
//                 await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);

//                 // Round trip the user through authorize to ensure claims are up to date.
//                 return RedirectToAction(nameof(ConnectAccountComplete), new { returnUrl });

//             }
//             catch (Exception ex)
//             {
//                 SaveMessage = "Could not connect the account: " + ex.Message;
//                 MessageType = AlertType.Danger;
//                 return RedirectToAction(nameof(ConnectAccount));
//             }
//         }

//         [HttpGet]
//         [Authorize]
//         [Route("account/auth/connected")]
//         public virtual async Task<IActionResult> ConnectAccountComplete(string returnUrl)
//         {
//             if (!Engine.Auth0Enabled)
//             {
//                 return NotFound();
//             }
//             var user = await GetCurrentUserOrThrow();
//             var model = new ConnectAccountModel()
//             {
//                 LocalPicture = user.Avatar.LargeUrl,
//                 ReturnUrl = returnUrl,
//                 RemotePicture = User.GetClaimValue(HoodClaimTypes.RemotePicture)
//             };
//             return View(model);
//         }
//         #endregion

//         #region Auth0 - Disconnect Account
//         [HttpGet]
//         [Authorize]
//         [Route("account/auth/disconnect")]
//         public virtual async Task<IActionResult> DisconnectAccount(string accountId)
//         {
//             if (!Engine.Auth0Enabled)
//             {
//                 return NotFound();
//             }
//             var user = await GetCurrentUserOrThrow();
//             var accountToDisconnect = user.ConnectedAuth0Accounts.SingleOrDefault(a => a.UserId == accountId);
//             var model = new DisconnectAccountModel()
//             {
//                 LocalPicture = user.GetAvatar(),
//                 AccountId = accountId,
//                 RemotePicture = accountToDisconnect.Picture
//             };
//             return View(model);
//         }
//         [HttpPost]
//         [Authorize]
//         [Route("account/auth/disconnect-confirm")]
//         public virtual async Task<IActionResult> DisconnectAccountConfirm(DisconnectAccountModel model)
//         {
//             if (!Engine.Auth0Enabled)
//             {
//                 return NotFound();
//             }
//             try
//             {
//                 if (model.AccountId == User.GetUserId())
//                 {
//                     throw new Exception("You cannot delete the account you are currently using to sign in.");
//                 }

//                 // connect the accounts.     
//                 var authService = new Auth0Service();
//                 var user = await GetCurrentUserOrThrow();
//                 var linkedUser = await authService.GetUserByAuth0UserId(model.AccountId);
//                 if (linkedUser == null)
//                 {
//                     throw new Exception("Your remote sign in method could not be located.");
//                 }
//                 var primaryAccount = user.GetPrimaryIdentity();
//                 if (primaryAccount == null)
//                 {
//                     throw new Exception("Could not load your primary sign in method.");
//                 }

//                 var identityToDisconnect = user.ConnectedAuth0Accounts.SingleOrDefault(a => a.UserId == model.AccountId);
//                 if (identityToDisconnect == null)
//                 {
//                     throw new Exception("Could not load your sign in method to remove.");
//                 }
//                 if (primaryAccount.Id == identityToDisconnect.Id)
//                 {
//                     throw new Exception("You cannot remove your primary sign in method.");
//                 }
//                 if (identityToDisconnect.Id == User.GetUserId())
//                 {
//                     throw new Exception("You cannot remove your current sign in method.");
//                 }
//                 if (Engine.Settings.Account.DeleteRemoteAccounts)
//                 {
//                     // de-link the accounts.
//                     try
//                     {
//                         var client = await authService.GetClientAsync();
//                         var response = await client.Users.UnlinkAccountAsync(primaryAccount.Id, identityToDisconnect.Provider, identityToDisconnect.UserId);
//                     }
//                     catch (ErrorApiException ex)
//                     {
//                         throw new Exception("Could not unlink your remote accounts: " + ex.Message);
//                     }

//                     await authService.DeleteUser(identityToDisconnect.Id);
//                 }
//                 await authService.DeleteLocalAuthIdentity(identityToDisconnect.Id);

//                 return RedirectToAction(nameof(DisconnectAccountComplete));
//             }
//             catch (Exception ex)
//             {
//                 SaveMessage = ex.Message;
//                 MessageType = AlertType.Danger;
//                 return RedirectToAction(nameof(DisconnectAccount), new { accountId = model.AccountId });
//             }
//         }

//         [HttpGet]
//         [Authorize]
//         [Route("account/auth/disconnected")]
//         public virtual async Task<IActionResult> DisconnectAccountComplete()
//         {
//             if (!Engine.Auth0Enabled)
//             {
//                 return NotFound();
//             }
//             var user = await GetCurrentUserOrThrow();
//             return View();
//         }
//         #endregion

//         #region Lockout / Access Denied / Registration Closed
//         [HttpGet]
//         [AllowAnonymous]
//         [Route("account/registration-closed")]
//         public virtual IActionResult RegistrationClosed(string returnUrl = null)
//         {
//             AccountSettings accountSettings = Engine.Settings.Account;
//             if (accountSettings.EnableRegistration)
//             {
//                 return RedirectToAction(nameof(Register), new { returnUrl });
//             }
//             return View();
//         }

//         [HttpGet]
//         [AllowAnonymous]
//         [Route("account/lockout")]
//         public virtual IActionResult Lockout()
//         {
//             return View();
//         }

//         [HttpGet]
//         [AllowAnonymous]
//         [Route("account/access-denied")]
//         public virtual IActionResult AccessDenied(string returnUrl)
//         {
//             if (User.Identity.IsAuthenticated && User.RequiresConnection())
//             {
//                 return RedirectToAction(nameof(ConnectAccount));
//             }
//             if (User.Identity.IsAuthenticated && !User.IsActive())
//             {
//                 return RedirectToAction(nameof(ConfirmRequired));
//             }
//             Response.StatusCode = 403;
//             return View();
//         }

//         #endregion

//         #region Confirm Email
//         [HttpGet]
//         [Authorize]
//         [Route("account/email/confirm-required")]
//         public virtual IActionResult ConfirmRequired()
//         {
//             return View(new ConfirmRequiredModel());
//         }

//         [HttpGet]
//         [Authorize]
//         [DisableForAuth0]
//         [Route("account/email/confirm")]
//         public virtual async Task<IActionResult> ConfirmEmail(string userId, string code)
//         {
//             if (userId == null || code == null)
//             {
//                 return RedirectToAction(nameof(HomeController<TContext>.Index), "Home");
//             }
//             ApplicationUser user = await _account.GetUserByIdAsync(userId);
//             if (user == null)
//             {
//                 throw new ApplicationException($"Unable to load user with ID '{userId}'.");
//             }

//             IdentityResult result = await _account.ConfirmEmailAsync(user, code);
//             if (!result.Succeeded)
//             {
//                 throw new Exception("Your email address could not be confirmed, the link you have clicked is invalid, perhaps it has expired. You can log in to resend a new verification email.");
//             }

//             if (user.Active)
//             {
//                 if (User.Identity.IsAuthenticated)
//                 {
//                     SaveMessage = "Your email address has been successfully validated.";
//                     RedirectToAction(nameof(AccountController.Index), "Manage");
//                 }
//             }
//             else
//             {
//                 user.Active = true;
//                 await _account.UpdateUserAsync(user);
//             }
//             return View("ConfirmEmail");
//         }

//         [HttpGet]
//         [Authorize]
//         [Route("account/email/resend-confirmation")]
//         public virtual async Task<IActionResult> ResendConfirm()
//         {
//             try
//             {

//                 IUserProfile user = await GetCurrentUserOrThrow();
//                 if (Engine.Auth0Enabled)
//                 {
//                     await SendAuth0VerificationEmail(user, User.GetUserId(), Url.AbsoluteAction("Login", "Account"));
//                 }
//                 else
//                 {
//                     await SendVerificationEmail(user, User.GetUserId(), Url.AbsoluteAction("Login", "Account"));
//                 }
//                 SaveMessage = $"Email verification has been resent.";
//                 MessageType = AlertType.Success;

//             }
//             catch (Exception ex)
//             {
//                 SaveMessage = $"Error sending an email verification: {ex.Message}";
//                 MessageType = AlertType.Danger;
//                 await _logService.AddExceptionAsync<AccountController>($"Error when sending an email verification.", ex);
//             }

//             return RedirectToAction(nameof(ConfirmRequired));
//         }

//         #endregion

//         #region Change Password
//         [HttpGet]
//         [Authorize(Policies.Active)]
//         [Route("account/change-password")]
//         public virtual IActionResult ChangePassword()
//         {
//             if (Engine.Auth0Enabled)
//             {
//                 return View("ChangePasswordAuth0");
//             }
//             return View(new ChangePasswordViewModel());
//         }

//         [HttpPost]
//         [DisableForAuth0]
//         [Authorize(Policies.Active)]
//         [ValidateAntiForgeryToken]
//         [Route("account/change-password")]
//         public virtual async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
//         {
//             if (!ModelState.IsValid)
//             {
//                 return View(model);
//             }

//             IUserProfile user = await GetCurrentUserOrThrow();
//             var changePasswordResult = await _account.ChangePassword(user, model.OldPassword, model.NewPassword);
//             if (!changePasswordResult.Succeeded)
//             {
//                 foreach (var error in changePasswordResult.Errors)
//                 {
//                     ModelState.AddModelError(string.Empty, error.Description);
//                 }
//                 return View(model);
//             }

//             var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
//             await signInManager.SignInAsync(user, isPersistent: false);

//             await _logService.AddLogAsync<AccountController>($"User ({user.UserName}) changed their password successfully.");

//             MessageType = AlertType.Success;
//             SaveMessage = "Your password has been changed.";

//             return RedirectToAction(nameof(ChangePassword));
//         }

//         [HttpPost]
//         [Authorize(Policies.Active)]
//         [ValidateAntiForgeryToken]
//         [Route("account/change-password/sent")]
//         public virtual async Task<IActionResult> ChangePasswordSentAsync(ChangePasswordViewModel model)
//         {
//             var user = await GetCurrentUserOrThrow();
//             var auth0Service = new Auth0Service();
//             var client = await auth0Service.GetClientAsync();
//             // get currently signed in user.
//             var userId = User.GetUserId();
//             Auth0Identity remoteUser = user.ConnectedAuth0Accounts.SingleOrDefault(ca => ca.Id == userId);
//             if (remoteUser == null)
//             {
//                 SaveMessage = "Could not send a password change as you are using a passwordless connection.";
//                 MessageType = AlertType.Warning;
//                 return View(new ChangePasswordViewModel());
//             }
//             if (remoteUser.Provider == Constants.AuthProviderName)
//             {
//                 var ticket = client.Tickets.CreatePasswordChangeTicketAsync(new Auth0.ManagementApi.Models.PasswordChangeTicketRequest()
//                 {
//                     UserId = remoteUser.UserId
//                 });
//             }
//             else
//             {
//                 switch (remoteUser.Provider)
//                 {
//                     case "email":
//                         SaveMessage = "Could not send a password change as you are using a passwordless connection.";
//                         MessageType = AlertType.Warning;
//                         break;
//                     default:
//                         SaveMessage = "Could not send a password change as you are using an external login account. Please change your password there and then sign out and back in again to update your login security.";
//                         MessageType = AlertType.Warning;
//                         break;
//                 }
//             }
//             return View(new ChangePasswordViewModel());
//         }


//         #endregion

//         #region Forgot / Reset Password

//         [HttpGet]
//         [AllowAnonymous]
//         [DisableForAuth0]
//         [Route("account/forgot-password")]
//         public virtual IActionResult ForgotPassword()
//         {
//             return View();
//         }

//         [HttpPost]
//         [AllowAnonymous]
//         [DisableForAuth0]
//         [ValidateAntiForgeryToken]
//         [Route("account/forgot-password")]
//         public virtual async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
//         {
//             if (ModelState.IsValid)
//             {
//                 ApplicationUser user = await _account.GetUserByEmailAsync(model.Email);
//                 if (user == null)
//                 {
//                     // Don't reveal that the user does not exist or is not confirmed
//                     return View("ForgotPasswordConfirmation");
//                 }

//                 await SendPasswordResetToken(user);

//                 return RedirectToAction(nameof(ForgotPasswordConfirmation));
//             }

//             // If we got this far, something failed, redisplay form
//             return View(model);
//         }

//         [HttpGet]
//         [AllowAnonymous]
//         [DisableForAuth0]
//         [Route("account/forgot-password/confirm")]
//         public virtual IActionResult ForgotPasswordConfirmation()
//         {
//             return View();
//         }

//         [HttpGet]
//         [AllowAnonymous]
//         [DisableForAuth0]
//         [Route("account/reset-password")]
//         public virtual IActionResult ResetPassword(string code = null)
//         {
//             if (code == null)
//             {
//                 throw new ApplicationException("A code must be supplied for password reset.");
//             }
//             ResetPasswordViewModel model = new ResetPasswordViewModel { Code = code };
//             return View(model);
//         }

//         [HttpPost]
//         [AllowAnonymous]
//         [DisableForAuth0]
//         [ValidateAntiForgeryToken]
//         [Route("account/reset-password")]
//         public virtual async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
//         {
//             if (!ModelState.IsValid)
//             {
//                 return View(model);
//             }
//             ApplicationUser user = await _account.GetUserByEmailAsync(model.Email);
//             if (user == null)
//             {
//                 // Don't reveal that the user does not exist
//                 return RedirectToAction("ResetPasswordConfirmation", "Account");
//             }
//             IdentityResult result = await _account.ResetPasswordAsync(user, model.Code, model.Password);
//             if (result.Succeeded)
//             {
//                 return RedirectToAction("ResetPasswordConfirmation", "Account");
//             }
//             AddErrors(result);
//             return View();
//         }

//         [HttpGet]
//         [AllowAnonymous]
//         [DisableForAuth0]
//         [Route("account/reset-password/confirm")]
//         public virtual IActionResult ResetPasswordConfirmation()
//         {
//             return View();
//         }

//         #endregion

//         #region Delete Account
//         [HttpGet]
//         [Authorize]
//         [Route("account/delete")]
//         public virtual IActionResult Delete()
//         {
//             return View(nameof(Delete), new SaveableModel());
//         }

//         [HttpPost]
//         [Authorize]
//         [ValidateAntiForgeryToken]
//         [Route("account/delete/confirm")]
//         public virtual async Task<IActionResult> ConfirmDelete()
//         {
//             try
//             {

//                 IUserProfile user = await GetCurrentUserOrThrow();
//                 await _account.DeleteUserAsync(user.Id, User);

//                 if (Engine.Auth0Enabled)
//                 {
//                     return RedirectToAction(nameof(SignOut), new { returnUrl = Url.Action(nameof(Deleted)) });
//                 }
//                 else
//                 {
//                     var signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
//                     await signInManager.SignOutAsync();
//                 }

//                 await _logService.AddLogAsync<AccountController>($"User with Id {user.Id} has deleted their account.");
//                 return RedirectToAction(nameof(Deleted));
//             }
//             catch (Exception ex)
//             {
//                 SaveMessage = $"Error deleting your account: {ex.Message}";
//                 MessageType = AlertType.Danger;
//                 await _logService.AddExceptionAsync<AccountController>($"Error when user attemted to delete their account.", ex);
//             }
//             return RedirectToAction(nameof(Delete));
//         }

//         [AllowAnonymous]
//         [Route("account/deleted")]
//         public virtual IActionResult Deleted()
//         {
//             return View(nameof(Deleted));
//         }
//         #endregion

//         #region Helpers

//         protected virtual async Task<IUserProfile> GetCurrentUserOrThrow()
//         {
//             var user = await _account.GetUserProfileByIdAsync(User.GetLocalUserId());
//             if (user == null)
//             {
//                 throw new ApplicationException($"Unable to load user with email '{User.GetEmail()}'.");
//             }
//             return user;
//         }

//         protected virtual async Task SendVerificationEmail(ApplicationUser localUser, string userId, string returnUrl)
//         {
//             var code = await _userManager.GenerateEmailConfirmationTokenAsync(localUser);
//             var contextAccessor = Engine.Services.Resolve<IHttpContextAccessor>();
//             if (contextAccessor == null)
//             {
//                 return;
//             }
//             var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
//             if (linkGenerator == null)
//             {
//                 return;
//             }
//             var callbackUrl = linkGenerator.GetUriByAction(contextAccessor.HttpContext, "ConfirmEmail", "Account", new { userId = localUser.Id, code, returnUrl });
//             var verifyModel = new VerifyEmailModel(localUser, callbackUrl)
//             {
//                 SendToRecipient = true
//             };
//             await _mailService.ProcessAndSend(verifyModel);
//         }

//         protected virtual async Task SendAuth0VerificationEmail(ApplicationUser localUser, string userId, string returnUrl)
//         {
//             // get the users' current connected account.
//             // send a verification email on the whattheolddowntheold.
//             var authService = new Auth0Service();
//             var ticket = await authService.GetEmailVerificationTicket(userId, returnUrl);
//             var verifyModel = new VerifyEmailModel(localUser, ticket.Value)
//             {
//                 SendToRecipient = true
//             };
//             await _mailService.ProcessAndSend(verifyModel);
//         }

//         protected virtual async Task SendPasswordResetToken(ApplicationUser user)
//         {
//             string code = await _userManager.GeneratePasswordResetTokenAsync(user);

//             var contextAccessor = Engine.Services.Resolve<IHttpContextAccessor>();
//             if (contextAccessor == null)
//             {
//                 return;
//             }
//             var linkGenerator = Engine.Services.Resolve<LinkGenerator>();
//             if (linkGenerator == null)
//             {
//                 return;
//             }

//             string callbackUrl = linkGenerator.GetUriByAction(contextAccessor.HttpContext, "ResetPassword", "Account", new { userId = user.Id, code });

//             MailObject message = new MailObject()
//             {
//                 To = new SendGrid.Helpers.Mail.EmailAddress(user.Email),
//                 PreHeader = "Reset your password.",
//                 Subject = "Reset your password."
//             };
//             message.AddParagraph($"Please reset your password by clicking here:");
//             message.AddCallToAction("Reset your password", callbackUrl);
//             message.Template = MailSettings.WarningTemplate;
//             await _emailSender.SendEmailAsync(message);
//         }

//         protected virtual async Task SendWelcomeEmail(ApplicationUser user)
//         {
//             string loginLink = Url.Action("Login", "Account", null, protocol: HttpContext.Request.Scheme);
//             WelcomeEmailModel welcomeModel = new WelcomeEmailModel(user, loginLink)
//             {
//                 SendToRecipient = true,
//                 NotifyRole = Engine.Settings.Account.NotifyNewAccount ? "NewAccountNotifications" : null
//             };
//             await _mailService.ProcessAndSend(welcomeModel);
//         }

//         protected virtual void AddErrors(IdentityResult result)
//         {
//             foreach (IdentityError error in result.Errors)
//             {
//                 ModelState.AddModelError(string.Empty, error.Description);
//             }
//         }

//         protected virtual IActionResult RedirectToLocal(string returnUrl)
//         {
//             if (Url.IsLocalUrl(returnUrl))
//             {
//                 return Redirect(returnUrl);
//             }
//             else
//             {
//                 return RedirectToAction("Index", "Home");
//             }
//         }

//         protected virtual IActionResult RedirectWithReturnUrl(string url, string returnUrl)
//         {
//             if (returnUrl != null)
//             {
//                 url += "&returnUrl=" + System.Net.WebUtility.UrlEncode(returnUrl);
//             }

//             return RedirectToLocal(url);
//         }

//         #endregion
//     }
// }