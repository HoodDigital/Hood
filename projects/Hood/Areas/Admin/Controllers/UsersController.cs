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
using Microsoft.EntityFrameworkCore;
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
        {}

        [Route("admin/users/")]
        public async Task<IActionResult> Index(UserListModel model, EditorMessage? message)
        {
            IList<ApplicationUser> users = new List<ApplicationUser>();
            if (!string.IsNullOrEmpty(model.Role))
            {
                users = await _userManager.GetUsersInRoleAsync(model.Role);
            }
            else
            {
                users = await _userManager.Users.ToListAsync();
            }
            if (!string.IsNullOrEmpty(model.Search))
            {
                string[] searchTerms = model.Search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                users = users.Where(n => searchTerms.Any(s => n.UserName.ToLower().Contains(s.ToLower()))).ToList();
            }
            switch (model.Order)
            {
                case "UserName":
                    users = users.OrderBy(n => n.UserName).ToList();
                    break;
                case "Email":
                    users = users.OrderBy(n => n.Email).ToList();
                    break;
                case "LastName":
                    users = users.OrderBy(n => n.LastName).ToList();
                    break;
                case "LastLogOn":
                    users = users.OrderByDescending(n => n.LastLogOn).ToList();
                    break;

                case "UserNameDesc":
                    users = users.OrderByDescending(n => n.UserName).ToList();
                    break;
                case "EmailDesc":
                    users = users.OrderByDescending(n => n.Email).ToList();
                    break;
                case "LastNameDesc":
                    users = users.OrderByDescending(n => n.LastName).ToList();
                    break;

                default:
                    users = users.OrderBy(n => n.UserName).ToList();
                    break;
            }
            model.Reload(users, model.PageIndex, model.PageSize);
            model.AddEditorMessage(message);
            return View(model);
        }

        #region Edit

        [Route("admin/users/edit/{id}/")]
        public async Task<IActionResult> Edit(string id)
        {
            UserProfile model = await _account.GetProfileAsync(id) as UserProfile;
            model.AllRoles = await _account.GetAllRolesAsync();
            return View(model);
        }

        [Route("admin/users/edit/{id}/")]
        [HttpPost]
        public async Task<IActionResult> Edit(UserProfile model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
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
                MessageType = Enums.AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = "There was an error while saving: " + ex.Message;
                MessageType = Enums.AlertType.Danger;
            }

            model.AllRoles = await _account.GetAllRolesAsync();

            return View(model);
        }

        #endregion       
        
        #region Create

        [Route("admin/users/create/")]
        public IActionResult Create()
        {
            return View();
        }

        [Route("admin/users/add/")]
        [HttpPost]
        public async Task<Response> Create(CreateUserModel model)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.cuUserName,
                    Email = model.cuUserName,
                    FirstName = model.cuFirstName,
                    LastName = model.cuLastName,
                    CreatedOn = DateTime.Now,
                    LastLogOn = DateTime.Now
                };
                user.AddUserNote(new UserNote()
                {
                    CreatedBy = User.GetUserId(),
                    CreatedOn = DateTime.Now,
                    Note = $"User created via admin panel by {User.Identity.Name}."
                });
                var result = await _userManager.CreateAsync(user, model.cuPassword);
                if (!result.Succeeded)
                {
                    return new Response(result.Errors);
                }
                if (model.cuNotifyUser)
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
                        message.AddParagraph("Username: <strong>" + model.cuUserName + "</strong>");
                        message.AddParagraph("Password: <strong>" + model.cuPassword + "</strong>");
                        message.AddCallToAction("Log in here", ControllerContext.HttpContext.GetSiteUrl() + "/account/login", "#32bc4e", "center");
                        await _emailSender.SendEmailAsync(message);
                    }
                    catch (Exception)
                    {
                        // roll back!
                        var deleteUser = await _userManager.FindByEmailAsync(model.cuUserName);
                        await _userManager.DeleteAsync(deleteUser);
                        throw new Exception("There was a problem sending the email, ensure the site's email address and SendGrid settings are set up correctly before sending.");
                    }
                }
                var response = new Response(true, "Published successfully.")
                {
                    Url = Url.Action("Edit", new { id = user.Id, message = EditorMessage.Created })
                };
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        #endregion

        #region Delete

        [Route("admin/users/delete/")]
        [HttpPost]
        public async Task<Response> Delete(string id)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                await _account.DeleteUserAsync(user);

                var response = new Response(true, "Deleted successfully.")
                {
                    Url = Url.Action("Index", new { message = EditorMessage.Deleted })
                };
                return response;

            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        #endregion

        #region Avatars

        [Route("admin/users/avatar/get/")]
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
        [Route("admin/users/avata/clear/")]
        [HttpGet]
        public async Task<Response> ClearAvatar(string id)
        {
            try
            {
                var user = await _account.GetUserByIdAsync(id);
                user.Avatar = null;
                await _account.UpdateUserAsync(user);
                return new Response(true, "The image has been cleared!");
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        #endregion

        #region Roles

        [Route("admin/users/getroles/")]
        [HttpGet]
        public async Task<JsonResult> GetRoles(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            IList<string> roles = await _userManager.GetRolesAsync(user);
            return Json(new { success = true, roles });
        }

        [Route("admin/users/addtorole/")]
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
                    return new Response(true);
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
                return new Response(ex.Message);
            }
        }

        [Route("admin/users/removefromrole/")]
        [HttpPost]
        public async Task<Response> RemoveFromRole(string id, string role)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                IdentityResult result = await _userManager.RemoveFromRoleAsync(user, role);
                if (result.Succeeded)
                {
                    return new Response(true);
                }
                else
                {
                    throw new Exception("The database could not be updated, please try later.");
                }
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        #endregion

        #region Impersonation
        public async Task<IActionResult> Impersonate(string id)
        {
            if (!id.IsSet())
                RedirectToAction("Index", new { message = EditorMessage.Error });

            var impersonatedUser = await _userManager.FindByIdAsync(id);
            var userPrincipal = await _signInManager.CreateUserPrincipalAsync(impersonatedUser);

            userPrincipal.Identities.First().AddClaim(new Claim("OriginalUserId", User.GetUserId()));
            userPrincipal.Identities.First().AddClaim(new Claim("IsImpersonating", "true"));

            // sign out the current user
            await _signInManager.SignOutAsync();

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, userPrincipal); // <-- This has changed from the previous version.

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [AllowAnonymous]
        public async Task<IActionResult> StopImpersonation()
        {
            if (!User.Identity.IsAuthenticated)
                throw new Exception("You are not impersonating now. Can't stop impersonation!");

            if (!User.IsImpersonating())
                throw new Exception("You are not impersonating now. Can't stop impersonation!");

            var originalUserId = User.FindFirst("OriginalUserId").Value;

            var originalUser = await _userManager.FindByIdAsync(originalUserId);

            await _signInManager.SignOutAsync();

            await _signInManager.SignInAsync(originalUser, isPersistent: true);

            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Password

        [Route("admin/users/reset/")]
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
                    return new Response(true);
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
                return new Response(ex.Message);
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
