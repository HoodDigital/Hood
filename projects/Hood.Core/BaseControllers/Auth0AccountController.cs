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
    public abstract class Auth0AccountController : BaseController
    {
        protected readonly IAuth0Service _auth0;
        protected readonly IAuth0AccountRepository _account;
        public Auth0AccountController()
        {
            _auth0 = Engine.Services.Resolve<IAuth0Service>();
            _account = Engine.Services.Resolve<IAuth0AccountRepository>();
        }

        #region Account Home

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
                Accounts = user.ConnectedAuth0Accounts,
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
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);

                SaveMessage = "Your profile has been updated.";
                MessageType = AlertType.Success;
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                SaveMessage = "Something went wrong: " + ex.Message;
                MessageType = AlertType.Danger;
                model.Accounts = await _account.GetUserAuth0IdentitiesById(model.Profile.Id);
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

        //     IUserProfile user = await GetCurrentUserOrThrow();
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
        //         return await ErrorResponseAsync<ManageController>($"There was an error setting the avatar.", ex);
        //     }
        // }

        #endregion

        #region Login 

        [HttpGet]
        [AllowAnonymous]
        [Route("account/login")]
        public virtual IActionResult Login(string returnUrl = null)
        {
            return RedirectToAction(nameof(Authorize), new { returnUrl, mode = "login" });
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
            return RedirectToAction(nameof(Authorize), new { returnUrl, mode = "signup" });
        }

        #endregion

        #region Logout

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("account/logout")]
        public virtual IActionResult LogOff(string returnUrl = "/")
        {
            return RedirectToAction(nameof(SignOut), new { returnUrl });
        }

        #endregion

        #region Auth0 - Sign in/out
        [HttpGet]
        [AllowAnonymous]
        [Route("account/authorize")]
        public virtual async Task Authorize(string returnUrl = "/", string mode = "login")
        {
            var authenticationPropertiesBuilder = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl);

            authenticationPropertiesBuilder = authenticationPropertiesBuilder.WithParameter("action", mode);

            var authenticationProperties = authenticationPropertiesBuilder.Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/auth/signout")]
        public virtual async Task SignOut(string returnUrl = "/")
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl.IsSet() ? returnUrl : Url.Action("Index", "Home"))
                .Build();

            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("account/auth/failed")]
        public virtual IActionResult RemoteSigninFailed(string r, string d)
        {
            switch (r)
            {
                case "unauthorized":
                    if (d.IsSet())
                    {
                        ViewData["Reason"] = "Unauthorized: " + d.ToTitleCase();
                    }
                    else
                    {
                        ViewData["Reason"] = "This account is not authorized.";
                    }
                    break;
                case "auth-failed":
                    ViewData["allow-relog"] = true;
                    ViewData["Reason"] = "Authentication failed, likely just a loose wire.";
                    break;
                case "state-failure":
                    ViewData["allow-relog"] = true;
                    ViewData["Reason"] = "Authentication failed, looks like you may be trying to log in with a different browser or device. Make sure you are using the same device to click your login link.";
                    break;
                case "remote-failed":
                    ViewData["allow-relog"] = true;
                    ViewData["Reason"] = "We could not sign you in due to a techical issue, likely just a loose wire.";
                    break;
                case "account-creation-failed":
                    ViewData["allow-relog"] = true;
                    ViewData["Reason"] = "We could not create a local account due to a techical issue, likely just a loose wire.";
                    break;
                case "account-linking-failed":
                    ViewData["allow-relog"] = true;
                    ViewData["Reason"] = "We could not connect your login to a local account due to a techical issue, likely just a loose wire.";
                    break;
            }
            return View();
        }
        #endregion

        #region Auth0 - Connect Account

        [HttpGet]
        [Authorize(Policies.AccountNotConnected)]
        [Route("account/auth/connect")]
        public virtual async Task<IActionResult> ConnectAccount(string returnUrl)
        {
            var user = await GetCurrentUserOrThrow();
            var model = new ConnectAccountModel()
            {
                LocalPicture = user.UserProfile.GetAvatar(),
                ReturnUrl = returnUrl,
                RemotePicture = User.GetClaimValue(Hood.Constants.Identity.ClaimTypes.RemotePicture)
            };
            if (User.HasClaim(Hood.Constants.Identity.ClaimTypes.AccountLinkRequired))
            {
                return View("ConnectAccountLink", model);
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Policies.AccountNotConnected)]
        [Route("account/auth/connect-confirm")]
        public virtual async Task<IActionResult> ConnectAccountConfirm(string returnUrl)
        {
            try
            {
                if (User.HasClaim(Hood.Constants.Identity.ClaimTypes.AccountLinkRequired))
                {
                    return RedirectToAction(nameof(ConnectAccount), new { returnUrl });
                }

                if (!User.IsEmailConfirmed())
                {
                    // tell the user they need to confirm their email, then try again. Restart flow.
                    return RedirectToAction(nameof(ConfirmRequired));
                }

                // connect the accounts.       

                var user = await GetCurrentUserOrThrow();
                var linkedUser = await _account.GetUserByAuth0Id(User.GetUserId());
                if (linkedUser != null)
                {
                    throw new Exception("This account is already connected to an account.");
                }

                var newAuthUser = await _account.CreateLocalAuthIdentity(User.GetUserId(), user, User.GetClaimValue(Hood.Constants.Identity.ClaimTypes.RemotePicture));
                if (newAuthUser == null)
                {
                    throw new Exception("There was a problem connecting your account, please try again.");
                }

                // Update local account profile from remote account info - if set and not set etc etc.
                if (user.UpdateFromPrincipal(User))
                {
                    await _account.UpdateUserAsync(user);
                }

                User.RemoveClaim(Hood.Constants.Identity.ClaimTypes.AccountNotConnected);
                User.AddOrUpdateClaimValue(Hood.Constants.Identity.ClaimTypes.Active, "true");
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);

                return RedirectToAction(nameof(ConnectAccountComplete), new { returnUrl });
            }
            catch (Exception ex)
            {
                SaveMessage = "Could not connect the account: " + ex.Message;
                MessageType = AlertType.Danger;
                return RedirectToAction(nameof(ConnectAccount));
            }
        }

        [HttpPost]
        [Authorize(Policies.AccountNotConnected)]
        [Route("account/auth/connect-link")]
        public virtual async Task<IActionResult> ConnectAccountLink(string returnUrl)
        {
            try
            {
                if (!User.HasClaim(Hood.Constants.Identity.ClaimTypes.AccountLinkRequired))
                {
                    return RedirectToAction(nameof(ConnectAccount), new { returnUrl });
                }

                if (!User.IsEmailConfirmed())
                {
                    // tell the user they need to confirm their email, then try again. Restart flow.
                    return RedirectToAction(nameof(ConfirmRequired));
                }

                // connect the accounts.             
                var user = await GetCurrentUserOrThrow();
                var linkedUser = await _account.GetUserByAuth0Id(User.GetUserId());
                if (linkedUser != null)
                {
                    throw new Exception("This account is already connected to an account.");
                }

                // get the user's other account....(s?)
                var primaryAccount = user.GetPrimaryIdentity();
                if (primaryAccount == null)
                {
                    throw new Exception("Could not load a primary account to link.");
                }

                try
                {
                    await _auth0.LinkAccountAsync(primaryAccount, User.GetUserId());

                    // Success - remove link claim.
                    User.RemoveClaim(Hood.Constants.Identity.ClaimTypes.AccountLinkRequired);
                }
                catch (ErrorApiException ex)
                {
                    if (ex.ApiError == null)
                    {
                        throw new Exception("Could not link the remote accounts: " + ex.Message);
                    }
                    switch (ex.ApiError.ErrorCode)
                    {
                        case "identity_conflict":
                            if (ex.ApiError.ExtraData.ContainsKey("statusCode") && ex.ApiError.ExtraData["statusCode"] == "409")
                            {
                                // Account already linked, remove link claim and continue.
                                User.RemoveClaim(Hood.Constants.Identity.ClaimTypes.AccountLinkRequired);
                            }
                            break;
                        default:
                            throw new Exception("Could not link the remote accounts: " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Could not link the remote accounts: " + ex.Message);
                }

                var newAuthUser = await _account.CreateLocalAuthIdentity(User.GetUserId(), user, User.GetClaimValue(Hood.Constants.Identity.ClaimTypes.RemotePicture));
                if (newAuthUser == null)
                {
                    throw new Exception("There was a problem connecting your account, please try again.");
                }

                User.AddOrUpdateClaimValue(Hood.Constants.Identity.ClaimTypes.Active, "true");
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, User);

                // Round trip the user through authorize to ensure claims are up to date.
                return RedirectToAction(nameof(ConnectAccountComplete), new { returnUrl });

            }
            catch (Exception ex)
            {
                SaveMessage = "Could not connect the account: " + ex.Message;
                MessageType = AlertType.Danger;
                return RedirectToAction(nameof(ConnectAccount));
            }
        }

        [HttpGet]
        [Authorize]
        [Route("account/auth/connected")]
        public virtual async Task<IActionResult> ConnectAccountComplete(string returnUrl)
        {
            var user = await GetCurrentUserOrThrow();
            var model = new ConnectAccountModel()
            {
                LocalPicture = user.UserProfile.GetAvatar(),
                ReturnUrl = returnUrl,
                RemotePicture = User.GetClaimValue(Hood.Constants.Identity.ClaimTypes.RemotePicture)
            };
            return View(model);
        }
        #endregion

        #region Auth0 - Disconnect Account
        [HttpGet]
        [Authorize]
        [Route("account/auth/disconnect")]
        public virtual async Task<IActionResult> DisconnectAccount(string accountId)
        {
            var user = await GetCurrentUserOrThrow();
            var accountToDisconnect = user.ConnectedAuth0Accounts.SingleOrDefault(a => a.LocalUserId == accountId);
            var model = new DisconnectAccountModel()
            {
                LocalPicture = user.UserProfile.GetAvatar(),
                AccountId = accountId,
                RemotePicture = accountToDisconnect.Picture
            };
            return View(model);
        }
        [HttpPost]
        [Authorize]
        [Route("account/auth/disconnect-confirm")]
        public virtual async Task<IActionResult> DisconnectAccountConfirm(DisconnectAccountModel model)
        {
            try
            {
                if (model.AccountId == User.GetUserId())
                {
                    throw new Exception("You cannot delete the account you are currently using to sign in.");
                }

                // connect the accounts.     
                var user = await GetCurrentUserOrThrow();
                var linkedUser = await _account.GetUserByAuth0Id(model.AccountId);
                if (linkedUser == null)
                {
                    throw new Exception("Your remote sign in method could not be located.");
                }
                var primaryAccount = user.GetPrimaryIdentity();
                if (primaryAccount == null)
                {
                    throw new Exception("Could not load your primary sign in method.");
                }

                var identityToDisconnect = user.ConnectedAuth0Accounts.SingleOrDefault(a => a.LocalUserId == model.AccountId);
                if (identityToDisconnect == null)
                {
                    throw new Exception("Could not load your sign in method to remove.");
                }
                if (primaryAccount.Id == identityToDisconnect.Id)
                {
                    throw new Exception("You cannot remove your primary sign in method.");
                }
                if (identityToDisconnect.Id == User.GetUserId())
                {
                    throw new Exception("You cannot remove your current sign in method.");
                }
                if (Engine.Settings.Account.DeleteRemoteAccounts)
                {
                    // de-link the accounts.
                    try
                    {
                        await _auth0.UnlinkAccountAsync(primaryAccount, identityToDisconnect);
                    }
                    catch (ErrorApiException ex)
                    {
                        throw new Exception("Could not unlink your remote accounts: " + ex.Message);
                    }

                    await _auth0.DeleteUser(identityToDisconnect.Id);
                }
                await _account.DeleteLocalAuthIdentity(identityToDisconnect.Id);

                return RedirectToAction(nameof(DisconnectAccountComplete));
            }
            catch (Exception ex)
            {
                SaveMessage = ex.Message;
                MessageType = AlertType.Danger;
                return RedirectToAction(nameof(DisconnectAccount), new { accountId = model.AccountId });
            }
        }

        [HttpGet]
        [Authorize]
        [Route("account/auth/disconnected")]
        public virtual async Task<IActionResult> DisconnectAccountComplete()
        {
            var user = await GetCurrentUserOrThrow();
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
        [Route("account/email/confirm-required")]
        public virtual IActionResult ConfirmRequired()
        {
            return View(new ConfirmRequiredModel());
        }

        [HttpGet]
        [Authorize]
        [Route("account/email/resend-confirmation")]
        public virtual async Task<IActionResult> ResendConfirm()
        {
            try
            {
                var user = await GetCurrentUserOrThrow();
                await SendAuth0VerificationEmail((Auth0User)user, User.GetUserId(), Url.AbsoluteAction("Login", "Account"));
                SaveMessage = $"Email verification has been resent.";
                MessageType = AlertType.Success;

            }
            catch (Exception ex)
            {
                SaveMessage = $"Error sending an email verification: {ex.Message}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<Auth0AccountController>($"Error when sending an email verification.", ex);
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
            return View("ChangePasswordAuth0");
        }

        [HttpPost]
        [Authorize(Policies.Active)]
        [ValidateAntiForgeryToken]
        [Route("account/change-password/sent")]
        public virtual async Task<IActionResult> ChangePasswordSentAsync(ChangePasswordViewModel model)
        {
            var user = await GetCurrentUserOrThrow();
            // get currently signed in user.
            var userId = User.GetUserId();
            Auth0Identity remoteUser = user.ConnectedAuth0Accounts.SingleOrDefault(ca => ca.Id == userId);
            if (remoteUser == null)
            {
                SaveMessage = "Could not send a password change as you are using a passwordless connection.";
                MessageType = AlertType.Warning;
                return View(new ChangePasswordViewModel());
            }
            if (remoteUser.Provider == Authentication.AuthProviderName)
            {
                var ticket = await _auth0.CreatePasswordChangeTicket(remoteUser);
            }
            else
            {
                switch (remoteUser.Provider)
                {
                    case "email":
                        SaveMessage = "Could not send a password change as you are using a passwordless connection.";
                        MessageType = AlertType.Warning;
                        break;
                    default:
                        SaveMessage = "Could not send a password change as you are using an external login account. Please change your password there and then sign out and back in again to update your login security.";
                        MessageType = AlertType.Warning;
                        break;
                }
            }
            return View(new ChangePasswordViewModel());
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

                if (Engine.Auth0Enabled)
                {
                    return RedirectToAction(nameof(SignOut), new { returnUrl = Url.Action(nameof(Deleted)) });
                }
                else
                {
                    var signInManager = Engine.Services.Resolve<SignInManager<Auth0User>>();
                    await signInManager.SignOutAsync();
                }

                await _logService.AddLogAsync<Auth0AccountController>($"User with Id {user.Id} has deleted their account.");
                return RedirectToAction(nameof(Deleted));
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error deleting your account: {ex.Message}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<Auth0AccountController>($"Error when user attemted to delete their account.", ex);
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

        protected virtual async Task<Auth0User> GetCurrentUserOrThrow()
        {
            var user = await _account.GetUserByIdAsync(User.GetLocalUserId());
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with email '{User.GetEmail()}'.");
            }
            return user;
        }

        protected virtual async Task SendAuth0VerificationEmail(Auth0User localUser, string userId, string returnUrl)
        {
            // get the users' current connected account.
            // send a verification email on the whattheolddowntheold.
            var ticket = await _auth0.GetEmailVerificationTicket(userId, returnUrl);
            var verifyModel = new VerifyEmailModel(localUser.UserProfile, ticket.Value)
            {
                SendToRecipient = true
            };
            await _mailService.ProcessAndSend(verifyModel);
        }

        #endregion
    }
}