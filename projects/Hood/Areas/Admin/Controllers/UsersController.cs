using Hood.Core;
using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using MailChimp.Net;
using MailChimp.Net.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "SuperUser,Admin")]
    public class UsersController : BaseController
    {
        public UsersController()
            : base()
        { }

        [Route("admin/users/")]
        public async Task<IActionResult> Index(UserListModel model) => await List(model, "Index");

        [HttpGet]
        [Route("admin/users/list/")]
        public async Task<IActionResult> List(UserListModel model, string viewName = "_List_Users")
        {
            model = await _account.GetUserProfilesAsync(model);
            return View(viewName, model);
        }

        #region Edit
        [Route("admin/users/{id}/edit/")]
        public async Task<IActionResult> Edit(string id)
        {
            UserProfile model = await _account.GetProfileAsync(id);
            await LoadAndCheckProfile(model);
            return View(model);
        }

        [Route("admin/users/{id}/edit/")]
        [HttpPost]
        public async Task<IActionResult> Edit(UserProfile model)
        {
            var user = await _account.GetUserByIdAsync(model.Id);
            try
            {
                var email = user.Email;
                if (model.Email != email)
                {
                    var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                    if (!setEmailResult.Succeeded)
                    {
                        throw new Exception(setEmailResult.Errors.FirstOrDefault().Description);
                    }
                }

                var phoneNumber = user.PhoneNumber;
                if (model.PhoneNumber != phoneNumber)
                {
                    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        model.Email = phoneNumber;
                        throw new Exception(setPhoneResult.Errors.FirstOrDefault().Description);
                    }
                }

                await _account.UpdateProfileAsync(model);

                SaveMessage = "Saved!";
                MessageType = AlertType.Success;
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
            catch (Exception ex)
            {
                SaveMessage = "There was an error while saving: " + ex.Message;
                MessageType = AlertType.Danger;

                await LoadAndCheckProfile(model);
                return View(model);
            }
        }
        [Route("admin/users/{id}/stripe/link/")]
        public async Task<IActionResult> LinkToStripe(string id, string customerId)
        {
            try
            {
                var user = await _account.GetUserByIdAsync(id);
                var customer = await _account.GetCustomerObjectAsync(user.StripeId);
                if (customer == null)
                {
                    customer = await _account.GetCustomerObjectAsync(customerId);
                    if (customer == null)
                        throw new Exception("The customer object does could not be validated.");
                    if (customer.Email.ToLower() != user.Email.ToLower())
                        throw new Exception("The email for the customer object does not match the user's email.");

                    user.StripeId = customerId;
                    await _account.UpdateUserAsync(user);
                    SaveMessage = "Account has been successfully linked with the Stripe customer account.";
                    MessageType = AlertType.Success;
                }
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error linking the customer object";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<UsersController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Edit), new { id });
        }
        private async Task LoadAndCheckProfile(UserProfile model)
        {
            model.AllRoles = await _account.GetAllRolesAsync();
            model.AccessCodes = await _account.GetAccessCodesAsync(model.Id);
            model.Customer = await _account.GetCustomerObjectAsync(model.StripeId);
            if (model.Customer == null)
                model.MatchedCustomerObjects = await _account.GetMatchingCustomerObjectsAsync(model.Email);
        }
        #endregion

        #region Create
        [Route("admin/users/create/")]
        public IActionResult Create()
        {
            return View();
        }

        [Route("admin/users/create/")]
        [HttpPost]
        public async Task<Response> Create(RegisterViewModel model)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DisplayName = model.DisplayName,
                    PhoneNumber = model.Phone,
                    JobTitle = model.JobTitle,
                    Anonymous = model.Anonymous,
                    CreatedOn = DateTime.Now,
                    LastLogOn = DateTime.Now,
                    LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString(),
                    LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString()
                };
                user.AddUserNote(new UserNote()
                {
                    CreatedBy = User.GetUserId(),
                    CreatedOn = DateTime.Now,
                    Note = $"User created via admin panel by {User.Identity.Name}."
                });
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return new Response(result.Errors);
                }
                if (model.NotifyUser)
                {
                    // Send the email to the user, letting em know n' shit.
                    // Create the email content
                    try
                    {
                        MailObject message = new MailObject()
                        {
                            To = new SendGrid.Helpers.Mail.EmailAddress(user.Email),
                            PreHeader = "You access information for " + Engine.Settings.Basic.FullTitle,
                            Subject = "You account has been created."
                        };
                        message.AddH1("Account Created!");
                        message.AddParagraph("Your new account has been set up on the " + Engine.Settings.Basic.FullTitle + " website.");
                        message.AddParagraph("Name: <strong>" + user.ToFullName() + "</strong>");
                        message.AddParagraph("Username: <strong>" + model.Username + "</strong>");
                        message.AddParagraph("Password: <strong>" + model.Password + "</strong>");
                        message.AddCallToAction("Log in here", ControllerContext.HttpContext.GetSiteUrl() + "/account/login", "#32bc4e", "center");
                        await _emailSender.SendEmailAsync(message);
                    }
                    catch (Exception)
                    {
                        // roll back!
                        var deleteUser = await _userManager.FindByEmailAsync(model.Username);
                        await _userManager.DeleteAsync(deleteUser);
                        throw new Exception("There was a problem sending the email, ensure the site's email address and SendGrid settings are set up correctly before sending.");
                    }
                }
                await _logService.AddLogAsync<UsersController>($"A new user account has been created in the admin area for {user.Email}", type: LogType.Success);
                return new Response(true, "Published successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error creating a user via the admin panel.", ex);
            }
        }
        #endregion

        #region Delete
        [Route("admin/users/{id}/delete/")]
        [HttpPost]
        public async Task<Response> Delete(string id)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                await _account.DeleteUserAsync(user);
                await _logService.AddLogAsync<UsersController>($"A new user account has been deleted in the admin area for {user.Email}", type: LogType.Warning);
                return new Response(true, "Deleted successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error deleting a user via the admin panel.", ex);
            }
        }
        #endregion

        #region Notes 
        [Route("admin/users/{id}/notes/")]
        public async Task<IActionResult> Notes(string id)
        {
            UserProfile model = await _account.GetProfileAsync(id);
            return View("_Inline_Notes", model);
        }

        [Route("admin/users/{id}/notes/add/")]
        [HttpPost]
        public async Task<Response> AddNote(string id, string note)
        {
            try
            {
                var user = await _account.GetUserByIdAsync(id);
                user.AddUserNote(new UserNote()
                {
                    Id = Guid.NewGuid(),
                    Note = note,
                    CreatedBy = User.Identity.Name,
                    CreatedOn = DateTime.Now
                });
                await _account.UpdateUserAsync(user);
                return new Response(true, "The note has been saved.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error adding a user note via the admin panel.", ex);
            }
        }

        [Route("admin/users/{id}/notes/delete/")]
        public async Task<Response> DeleteNote(string id, Guid noteId)
        {
            try
            {
                var user = await _account.GetUserByIdAsync(id);
                var notes = user.Notes;
                var note = notes.SingleOrDefault(n => n.Id == noteId);
                if (note != null && notes.Contains(note))
                    notes.Remove(note);
                user.Notes = notes;
                await _account.UpdateUserAsync(user);
                return new Response(true, "The note has been deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error deleting a note from a user.", ex);
            }
        }
        #endregion

        #region Avatars
        [Route("admin/users/{id}/avatar/get/")]
        [HttpGet]
        public async Task<IMediaObject> GetAvatar(string id)
        {
            try
            {
                var user = await _account.GetUserByIdAsync(id);
                if (user != null)
                {
                    if (user.Avatar == null)
                        return MediaObject.Blank;
                    return new MediaObject(user.Avatar);
                }
                else
                    throw new Exception("No avatar found");
            }
            catch (Exception)
            {
                return MediaObject.Blank;
            }
        }
        [Route("admin/users/{id}/avatar/clear/")]
        [HttpGet]
        public async Task<Response> ClearAvatar(string id)
        {
            try
            {
                var user = await _account.GetUserByIdAsync(id);
                user.Avatar = null;
                await _account.UpdateUserAsync(user);
#warning TODO: Handle response in JS.
                return new Response(true, "The image has been cleared!");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error clearing a user's avatar via the admin panel.", ex);
            }
        }
        #endregion

        #region Roles
        [Route("admin/users/{id}/getroles/")]
        [HttpGet]
        public async Task<JsonResult> GetRoles(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            IList<string> roles = await _userManager.GetRolesAsync(user);
            return Json(new { success = true, roles });
        }
        [Route("admin/users/{id}/addtorole/")]
        [HttpPost]
        public async Task<Response> AddToRole(string id, string role)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

                IdentityResult result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    return new Response(true, $"The user has been added to the {role} role.");
                }
                else
                {
                    IdentityError error = result.Errors.FirstOrDefault();
                    if (error != null)
                    {
                        throw new Exception(error.Description);
                    }
                    throw new Exception("The database could not be updated, please try later.");
                }
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error adding a user to a role via the admin panel.", ex);
            }
        }
        [Route("admin/users/{id}/removefromrole/")]
        [HttpPost]
        public async Task<Response> RemoveFromRole(string id, string role)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                IdentityResult result = await _userManager.RemoveFromRoleAsync(user, role);
                if (result.Succeeded)
                {
                    return new Response(true, $"The user has been removed from the {role} role.");
                }
                else
                {
                    throw new Exception("The database could not be updated, please try later.");
                }
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error removing a user from a role via the admin panel.", ex);
            }
        }
        #endregion

        #region Impersonation
        public async Task<IActionResult> Impersonate(string id)
        {
            try
            {
                if (!id.IsSet())
                    throw new Exception("Cannot impersonate a user with no Id!");
                var impersonatedUser = await _userManager.FindByIdAsync(id);
                var userPrincipal = await _signInManager.CreateUserPrincipalAsync(impersonatedUser);

                userPrincipal.Identities.First().AddClaim(new Claim("OriginalUserId", User.GetUserId()));
                userPrincipal.Identities.First().AddClaim(new Claim("IsImpersonating", "true"));

                // sign out the current user
                await _signInManager.SignOutAsync();

                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, userPrincipal); // <-- This has changed from the previous version.

                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error impersonating user with {id}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<UsersController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Edit), new { id });
        }
        [AllowAnonymous]
        public async Task<IActionResult> StopImpersonation()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    throw new Exception("You are not logged in.");

                if (!User.IsImpersonating())
                    throw new Exception("You are not impersonating.");

                var originalUserId = User.FindFirst("OriginalUserId").Value;

                var originalUser = await _userManager.FindByIdAsync(originalUserId);

                await _signInManager.SignOutAsync();

                await _signInManager.SignInAsync(originalUser, isPersistent: true);

                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error stopping impersonating user";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<UsersController>(SaveMessage, ex);
            }
            return RedirectToAction("Index", "Home", new { area = "" });
        }
        #endregion

        #region Password
        [Route("admin/users/{id}/reset/")]
        [HttpPost]
        public async Task<Response> ResetPassword(string id, string password)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, password);
                if (result.Succeeded)
                {
                    await _logService.AddLogAsync<UsersController>($"The password has been reset by an admin for user with Id: {id}", type: LogType.Success);
                    return new Response(true, $"The user's password has been reset.");
                }
                else
                {
                    string error = "";
                    foreach (var err in result.Errors)
                    {
                        error += err.Description + Environment.NewLine;
                    }
                    throw new Exception(error);
                }
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error resetting a password via the admin panel for user with Id: {id}", ex);
            }
        }
        #endregion

        #region Mailchimp
        [Route("admin/users/sync/mailchimp/")]
        public async Task<IActionResult> SyncToMailchimp()
        {
            var stats = new MailchimpSyncStats();

            var integrations = Engine.Settings.Integrations;
            var mailchimpManager = new MailChimpManager(integrations.MailchimpApiKey);

            // delete users
            stats.MailchimpTotal = await mailchimpManager.Members.GetTotalItems(integrations.MailchimpUserListId, MailChimp.Net.Models.Status.Undefined).ConfigureAwait(false);
            var members = await mailchimpManager.Members.GetAllAsync(integrations.MailchimpUserListId, new MemberRequest()
            {
                Status = MailChimp.Net.Models.Status.Undefined
            }).ConfigureAwait(false);
            foreach (var member in members)
            {
                if (!_db.Users.Any(u => u.Email == member.EmailAddress))
                {
                    await mailchimpManager.Members.DeleteAsync(integrations.MailchimpUserListId, member.EmailAddress);
                    stats.Deleted++;
                }
            }

            // Add users
            stats.SiteTotal = _db.Users.Where(u => u.Email != null).Count();
            foreach (var user in _db.Users)
            {
                if (user.Email.IsSet())
                {
                    var exists = await mailchimpManager.Members.ExistsAsync(integrations.MailchimpUserListId, user.Email, falseIfUnsubscribed: false);
                    if (!exists)
                    {
                        var member = new MailChimp.Net.Models.Member()
                        {
                            EmailAddress = user.Email,
                            Status = MailChimp.Net.Models.Status.Subscribed,
                            StatusIfNew = MailChimp.Net.Models.Status.Subscribed
                        };
                        await mailchimpManager.Members.AddOrUpdateAsync(integrations.MailchimpUserListId, member);
                        stats.Added++;
                    }
                }
            }

            // show currently [unsubscribed] users
            var unsubscribed = await mailchimpManager.Members.GetAllAsync(integrations.MailchimpUserListId, new MemberRequest()
            {
                Status = MailChimp.Net.Models.Status.Unsubscribed
            }).ConfigureAwait(false);
            stats.UnsubscribedUsers = unsubscribed.Select(m => m.EmailAddress).ToList();
            return View(stats);
        }
        #endregion
    }
}
