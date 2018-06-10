using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Filters;
using Hood.Infrastructure;
using Hood.Interfaces;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using MailChimp.Net;
using MailChimp.Net.Core;
using MailChimp.Net.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class UsersController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        public UsersController()
            : base()
        {
        }

        [Route("admin/users/")]
        public async Task<IActionResult> Index(UserSearchModel model, EditorMessage? message)
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

        [Route("admin/users/edit/{id}/")]
        public async Task<IActionResult> Edit(string id)
        {
            EditUserModel um = new EditUserModel()
            {
                User = _auth.GetUserById(id)
            };
            um.Roles = await _userManager.GetRolesAsync(um.User);
            um.AllRoles = _auth.GetAllRoles();
            return View(um);
        }

        [Route("admin/users/edit/{id}/")]
        [HttpPost]
        public async Task<IActionResult> Edit(string id, SaveProfileModel model)
        {
            EditUserModel um = new EditUserModel()
            {
                User = _auth.GetUserById(id)
            };
            try
            {
                model.User.CopyProperties(um.User);
                _auth.UpdateUser(um.User);

                // reload model

                um.Roles = await _userManager.GetRolesAsync(um.User);
                um.AllRoles = _auth.GetAllRoles();
                um.SaveMessage = "Saved!";
                um.MessageType = Enums.AlertType.Success;
                return View(um);
            }
            catch (Exception ex)
            {
                um.SaveMessage = "There was an error while saving: " + ex.Message;
                um.MessageType = Enums.AlertType.Danger;
                return View(um);
            }
        }

        [Route("admin/users/blade/{id}/")]
        public async Task<IActionResult> Blade(string id)
        {
            EditUserModel um = new EditUserModel()
            {
                User = _auth.GetUserById(id)
            };
            um.Roles = await _userManager.GetRolesAsync(um.User);
            um.AllRoles = _auth.GetAllRoles();
            return View(um);
        }

        [Route("admin/users/create/")]
        public IActionResult Create()
        {
            return View();
        }

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

        [Route("admin/users/getaddresses/")]
        [HttpGet]
        public List<Address> GetAddresses()
        {
            var user = _auth.GetCurrentUser();
            return user.Addresses;
        }

        [Route("admin/users/getusernames/")]
        public List<string> GetUserNames(bool normalised = false)
        {
            if (normalised)
                return _db.Users.Select(u => u.NormalizedUserName).ToList();
            return _db.Users.Select(u => u.UserName).ToList();
        }

        [Route("admin/users/getavatar/")]
        [HttpGet]
        public IMediaObject GetAvatar(string id)
        {
            try
            {
                var user = _auth.GetUserById(id);
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

        [Route("admin/users/getroles/")]
        [HttpGet]
        public async Task<JsonResult> GetRoles(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            IList<string> roles = await _userManager.GetRolesAsync(user);
            return Json(new { success = true, roles = roles });
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

        [Route("admin/users/add/")]
        [HttpPost]
        public async Task<Response> Add(CreateUserModel model)
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
                    LastLogOn = DateTime.Now,
                    SystemNotes = string.Format("[{0}] User created via admin panel by {1}.", DateTime.Now, User.Identity.Name) + Environment.NewLine
                };

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
                            PreHeader = "You access information for " + _settings.GetSiteTitle(),
                            Subject = "You account has been created."
                        };
                        message.AddH1("Account Created!");
                        message.AddParagraph("Your account has been set up on the " + _settings.GetSiteTitle() + " website.");
                        message.AddParagraph("Name: <strong>" + user.FullName + "</strong>");
                        message.AddParagraph("Username: <strong>" + model.cuUserName + "</strong>");
                        message.AddParagraph("Password: <strong>" + model.cuPassword + "</strong>");
                        await _email.SendEmailAsync(message);
                    }
                    catch (Exception)
                    {
                        // roll back!
                        var deleteUser = await _userManager.FindByEmailAsync(model.cuUserName);
                        await _userManager.DeleteAsync(deleteUser);
                        throw new Exception("There was a problem sending the email, ensure the site's email address and SendGrid settings are set up correctly before sending.");
                    }
                }
                var response = new Response(true, "Published successfully.");
                response.Url = Url.Action("Edit", new { id = user.Id, message = EditorMessage.Created });
                return response;
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/users/clearimage/")]
        [HttpGet]
        public Response ClearImage(string id)
        {
            try
            {
                var user = _auth.GetUserById(id);
                user.Avatar = null;
                _auth.UpdateUser(user);
                return new Response(true, "The image has been cleared!");
            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

        [Route("admin/users/delete/")]
        [HttpPost()]
        public async Task<Response> Delete(string id)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                await _auth.DeleteUserAsync(user);

                var response = new Response(true, "Deleted successfully.");
                response.Url = Url.Action("Index", new { message = EditorMessage.Deleted });
                return response;

            }
            catch (Exception ex)
            {
                return new Response(ex.Message);
            }
        }

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

            return RedirectToAction("Index", "Home");
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

        [Route("admin/users/sync/mailchimp/")]
        public async Task<IActionResult> SyncToMailchimp()
        {
            var stats = new MailchimpSyncStats();

            var integrations = _settings.GetIntegrationSettings();
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
    }
}
