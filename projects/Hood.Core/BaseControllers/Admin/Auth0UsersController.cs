﻿using Hood.BaseControllers;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hood.Admin.BaseControllers
{
    [Authorize(Roles = "SuperUser,Admin")]
    public abstract class Auth0UsersController : BaseController
    {
        protected readonly IAuth0AccountRepository _account;
        protected readonly IAuth0Service _auth0;
        public Auth0UsersController()
            : base()
        {
            _account = Engine.Services.Resolve<IAuth0AccountRepository>();
            _auth0 = Engine.Services.Resolve<IAuth0Service>();
        }


        #region User List 

        [Route("admin/users/")]
        public virtual async Task<IActionResult> Index(UserListModel<UserProfileView<Auth0Role>> model)
        {
            return await List(model, "../Auth0Users/Index");
        }

        [HttpGet]
        [Route("admin/users/list/")]
        public virtual async Task<IActionResult> List(UserListModel<UserProfileView<Auth0Role>> model, string viewName = "../Auth0Users/_List_Users")
        {
            model = await _account.GetUserProfileViewsAsync(model);
            return View(viewName, model);
        }

        #endregion

        #region Edit
        [Route("admin/users/{id}/edit/")]
        public virtual async Task<IActionResult> Edit(string id)
        {
            UserProfileView<Auth0Role> model = await _account.GetUserProfileViewById(id);
            var roles = await _account.GetRolesAsync(new RoleListModel<Auth0Role> { PageIndex = 0, PageSize = int.MaxValue });
            model.AllRoles = roles.List;
            return View("../Auth0Users/Edit", model);
        }

        [Route("admin/users/{id}/edit/")]
        [HttpPost]
        public virtual async Task<IActionResult> Edit(UserProfileView<Auth0Role> model)
        {
            Auth0User modelToUpdate = await _account.GetUserByIdAsync(model.Id);
            try
            {
                var updatedFields = Request.Form.Keys.ToHashSet();
                modelToUpdate = modelToUpdate.UpdateFromFormModel(model, updatedFields);

                await _account.UpdateUserAsync(modelToUpdate);

                SaveMessage = "Saved!";
                MessageType = AlertType.Success;

                return RedirectToAction(nameof(Edit), new { id = model.Id });

            }
            catch (Exception ex)
            {
                SaveMessage = "There was an error while saving: " + ex.Message;
                MessageType = AlertType.Danger;

            }

            var roles = await _account.GetRolesAsync(new RoleListModel<Auth0Role> { PageIndex = 0, PageSize = int.MaxValue });
            model.AllRoles = roles.List;
            return View("../Auth0Users/Edit", model);
        }

        #endregion

        #region Create
        [Route("admin/users/create/")]
        public virtual IActionResult Create()
        {
            return View("../Auth0Users/Create");
        }

        [Route("admin/users/create/")]
        [HttpPost]
        public virtual async Task<Response> Create(AdminCreateUserViewModel model)
        {
            try
            {
                Auth0User user = new Auth0User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    PhoneNumber = model.Phone,
                    CreatedOn = DateTime.UtcNow,
                    LastLogOn = DateTime.UtcNow,
                    LastLoginLocation = HttpContext.Connection.RemoteIpAddress.ToString(),
                    LastLoginIP = HttpContext.Connection.RemoteIpAddress.ToString(),
                    UserProfile = new UserProfile
                    {
                        UserName = model.Username,
                        Email = model.Email,
                        PhoneNumber = model.Phone,

                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        DisplayName = model.DisplayName,
                        JobTitle = model.JobTitle,
                        Anonymous = model.Anonymous
                    }
                };
                user.UserProfile.AddUserNote(new UserNote()
                {
                    CreatedBy = User.GetLocalUserId(),
                    CreatedOn = DateTime.UtcNow,
                    Note = $"User created via admin panel by {User.Identity.Name}."
                });
                bool result = await _account.CreateAsync(user);
                if (!result)
                {
                    return new Response("Could not create the user.");
                }

                if (model.CreateValidated)
                {
                    user.EmailConfirmed = true;
                    user.Active = true;
                    await _account.UpdateUserAsync(user);
                }
                else
                {
                    if (!Engine.Settings.Account.RequireEmailConfirmation)
                    {
                        user.EmailConfirmed = true;
                    }
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
                        message.AddParagraph("Name: <strong>" + user.UserProfile.ToInternalName() + "</strong>");
                        message.AddParagraph("Username: <strong>" + model.Username + "</strong>");
                        message.AddParagraph("Password: <strong>" + model.Password + "</strong>");
                        message.AddCallToAction("Log in here", ControllerContext.HttpContext.GetSiteUrl() + "/account/login", "#32bc4e", "center");
                        await _emailSender.SendEmailAsync(message);
                    }
                    catch (Exception)
                    {
                        // roll back!
                        Auth0User deleteUser = await _account.GetUserByEmailAsync(model.Email);
                        await _account.DeleteUserAsync(deleteUser.Id, User);
                        throw new Exception("There was a problem sending the email, ensure the site's email address and SendGrid settings are set up correctly before sending.");
                    }
                }
                await _logService.AddLogAsync<UsersController>($"A new user account has been created in the admin area for {user.Email}", type: LogType.Success);
                return new Response(true, "Successfully created.");
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
        public virtual async Task<Response> Delete(string id)
        {
            try
            {
                Auth0User user = await _account.GetUserByIdAsync(id);
                if (user == null)
                {
                    throw new Exception($"The user Id {id} could not be found, therefore could not be deleted.");
                }

                await _account.DeleteUserAsync(id, User);
                await _logService.AddLogAsync<UsersController>($"The user account ({user.Email}) has been deleted via the admin area by {User.Identity.Name}", type: LogType.Warning);
                return new Response(true, "Deleted successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error deleting a user via the admin panel.", ex);
            }
        }
        #endregion

        #region MarkConfirmed
        [Route("admin/users/{id}/confirm-email/")]
        public virtual async Task<Response> MarkEmailConfirmed(string id)
        {
            try
            {
                Auth0User user = await _account.GetUserByIdAsync(id);
                if (user == null)
                {
                    throw new Exception($"The user Id {id} could not be found, therefore could not be confirmed.");
                }

                user.EmailConfirmed = true;
                if (!user.Active)
                {
                    user.Active = true;
                }
                user.UserProfile.AddUserNote(new UserNote()
                {
                    CreatedBy = User.GetLocalUserId(),
                    CreatedOn = DateTime.UtcNow,
                    Note = $"User email marked as confirmed via admin panel by {User.Identity.Name}."
                });
                await _account.UpdateUserAsync(user);

                await _logService.AddLogAsync<UsersController>($"The user account ({user.Email}) email confirmed via admin panel by {User.Identity.Name}", type: LogType.Warning);
                return new Response(true, "Email confirmed successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error confirming email for user via the admin panel.", ex);
            }
        }
        #endregion

        #region Notes 
        [Route("admin/users/{id}/notes/")]
        public virtual async Task<IActionResult> Notes(string id)
        {
            UserProfile model = await _account.GetUserProfileByIdAsync(id);
            return View("../Auth0Users/_Inline_Notes", model);
        }

        [Route("admin/users/{id}/notes/add/")]
        [HttpPost]
        public virtual async Task<Response> AddNote(string id, string note)
        {
            try
            {
                Auth0User user = await _account.GetUserByIdAsync(id);
                user.UserProfile.AddUserNote(new UserNote()
                {
                    Id = Guid.NewGuid(),
                    Note = note,
                    CreatedBy = User.Identity.Name,
                    CreatedOn = DateTime.UtcNow
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
        public virtual async Task<Response> DeleteNote(string id, Guid noteId)
        {
            try
            {
                Auth0User user = await _account.GetUserByIdAsync(id);
                List<UserNote> notes = user.UserProfile.Notes;
                UserNote note = notes.SingleOrDefault(n => n.Id == noteId);
                if (note != null && notes.Contains(note))
                {
                    notes.Remove(note);
                }

                user.UserProfile.Notes = notes;
                await _account.UpdateUserAsync(user);
                return new Response(true, "The note has been deleted.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error deleting a note from a user.", ex);
            }
        }
        #endregion

        #region Roles

        [Route("admin/roles/")]
        public virtual async Task<IActionResult> Roles(RoleListModel<Auth0Role> model)
        {
            return await ListRoles(model, "../Auth0Users/Roles");
        }

        [HttpGet]
        [Route("admin/roles/list/")]
        public virtual async Task<IActionResult> ListRoles(RoleListModel<Auth0Role> model, string viewName = "../Auth0Users/_List_Roles")
        {
            model = await _account.GetRolesAsync(model);
            var allRemoteRoles = await _auth0.GetRoles("", 0, 100);
            foreach (var role in model.List)
            {
                if (role.RemoteId.IsSet())
                {
                    var remote = allRemoteRoles.List.SingleOrDefault(r => r.Id == role.RemoteId);
                    role.RemoteRole = remote;
                }
            }
            return View(viewName, model);
        }

        // [Route("admin/roles/create/")]
        // public virtual IActionResult CreateRole()
        // {
        //     return View("../Auth0Users/CreateRole");
        // }

        // [Route("admin/roles/create/")]
        // [HttpPost]
        // public virtual async Task<Response> CreateRole(AdminCreateRoleViewModel model)
        // {
        //     try
        //     {
        //         await _account.CreateRoleAsync(model.Name);
        //         return new Response(true, "Successfully created.");
        //     }
        //     catch (Exception ex)
        //     {
        //         return await ErrorResponseAsync<Auth0UsersController>($"Error creating a role via the admin panel.", ex);
        //     }
        // }

        [Route("admin/roles/edit/")]
        public virtual async Task<IActionResult> EditRole(string roleName)
        {
            var role = await _account.GetRoleAsync(roleName);

            try
            {
                if (role.RemoteId.IsSet())
                {
                    role.RemoteRole = await _auth0.GetRoleById(role.RemoteId);
                }
            }
            catch (Exception)
            { }
            return View("../Auth0Users/EditRole", role);
        }

        [Route("admin/roles/delete/")]
        public virtual async Task<IActionResult> DeleteRole(string roleName)
        {
            try
            {
                Auth0Role role = await _account.GetRoleAsync(roleName);
                if (role == null)
                {
                    throw new Exception($"The role {roleName} could not be found, therefore could not be deleted.");
                }

                await _account.DeleteRoleAsync(roleName);

                SaveMessage = $"The role ({role.Name}) has been deleted via the admin area by {User.Identity.Name}";
                MessageType = AlertType.Danger;
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error deleting a role via the admin panel:" + ex;
                MessageType = AlertType.Danger;
            }
            return RedirectToAction("Roles");
        }

        [Route("admin/roles/link/")]
        public virtual async Task<IActionResult> LinkRole(string roleName)
        {
            try
            {
                Auth0Role role = await _account.GetRoleAsync(roleName);
                if (role == null)
                {
                    throw new Exception($"The role {roleName} could not be found, therefore could not be relinked.");
                }

                if (role.RemoteId.IsSet())
                {
                    try
                    {
                        role.RemoteRole = await _auth0.GetRoleById(role.RemoteId);
                    }
                    catch (System.Exception)
                    {}
                    if (role.RemoteRole != null)
                    {
                        throw new Exception("This role is already connected to a remote role. Remove the remote before re-linking.");
                    }
                }

                role.RemoteRole = await _auth0.CreateRoleForLocalRole(role);

                role.RemoteId = role.RemoteRole.Id;
                await _account.UpdateRoleAsync(role);

                SaveMessage = $"The role ({role.Name}) has been relinked to Auth0";
                MessageType = AlertType.Success;
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error relinking a role via the admin panel:" + ex.Message;
                MessageType = AlertType.Danger;
            }
            return RedirectToAction("Roles");
        }
        #endregion

        #region User Roles
        [Route("admin/users/{id}/get-roles/")]
        [HttpGet]
        public virtual async Task<JsonResult> GetRoles(string id)
        {
            Auth0User user = await _account.GetUserByIdAsync(id);
            IList<Auth0Role> roles = await _account.GetRolesForUser(user);
            return Json(new { success = true, roles });
        }
        [Route("admin/users/{id}/set-role/")]
        [HttpPost]
        public virtual async Task<Response> SetRole(string id, string role, bool add)
        {
            try
            {
                Auth0User user = await _account.GetUserByIdAsync(id);
                Auth0Role Auth0Role = await _account.GetRoleAsync(role);
                if (!await _account.RoleExistsAsync(role))
                {
                    await _account.CreateRoleAsync(role);
                }

                var roles = new List<Auth0Role>() {
                    Auth0Role
                };

                if (add)
                {
                    return await _account.AddUserToRolesAsync(user, roles.ToArray());
                }
                else
                {
                    return await _account.RemoveUserFromRolesAsync(user, roles.ToArray());
                }
            }
            catch (Exception ex)
            {
                if (add)
                {
                    return await ErrorResponseAsync<Auth0UsersController>($"Error adding a user to a role via the admin panel.", ex);
                }
                else
                {
                    return await ErrorResponseAsync<Auth0UsersController>($"Error removing a user from a role via the admin panel.", ex);
                }
            }
        }

        #endregion

        #region Impersonation
        public virtual async Task<IActionResult> Impersonate(string id)
        {
            try
            {
                if (Engine.Auth0Enabled)
                {
                    throw new ApplicationException("This feature is not supported by Hood CMS Auth0 yet.");
                }

                if (!id.IsSet())
                {
                    throw new Exception("Cannot impersonate a user with no Id!");
                }

                SignInManager<Auth0User> signInManager = Engine.Services.Resolve<SignInManager<Auth0User>>();
                Auth0User impersonatedUser = await _account.GetUserByIdAsync(id);
                ClaimsPrincipal userPrincipal = await signInManager.CreateUserPrincipalAsync(impersonatedUser);

                userPrincipal.Identities.First().AddClaim(new Claim(Hood.Constants.Identity.ClaimTypes.OriginalUserId, User.GetLocalUserId()));
                userPrincipal.Identities.First().AddClaim(new Claim(Hood.Constants.Identity.ClaimTypes.IsImpersonating, "true"));

                impersonatedUser.UserProfile.AddUserNote(new UserNote()
                {
                    CreatedBy = User.GetLocalUserId(),
                    CreatedOn = DateTime.UtcNow,
                    Note = $"User {impersonatedUser.UserName} was impersonated by {User.Identity.Name}."
                });
                await _account.UpdateUserAsync(impersonatedUser);

                // sign out the current user
                await signInManager.SignOutAsync();

                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, userPrincipal); // <-- This has changed from the previous version.

                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error impersonating user with {id}";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<Auth0UsersController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Edit), new { id });
        }

        [AllowAnonymous]
        public virtual async Task<IActionResult> StopImpersonation()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    throw new Exception("You are not logged in.");
                }

                if (!User.IsImpersonating())
                {
                    throw new Exception("You are not impersonating.");
                }

                string originalUserId = User.FindFirst(Hood.Constants.Identity.ClaimTypes.OriginalUserId).Value;

                Auth0User originalUser = await _account.GetUserByIdAsync(originalUserId);

                SignInManager<Auth0User> signInManager = Engine.Services.Resolve<SignInManager<Auth0User>>();
                await signInManager.SignOutAsync();
                await signInManager.SignInAsync(originalUser, isPersistent: true);

                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {
                SaveMessage = $"Error stopping impersonating user";
                MessageType = AlertType.Danger;
                await _logService.AddExceptionAsync<Auth0UsersController>(SaveMessage, ex);
            }
            return RedirectToAction("Index", "Home", new { area = "" });
        }
        #endregion

        #region Password
        [Route("admin/users/{id}/reset/")]
        [HttpPost]
        public virtual async Task<Response> ResetPassword(string id, string password)
        {
            try
            {
                if (Engine.Auth0Enabled)
                {
                    throw new ApplicationException("This feature is not supported by Hood CMS Auth0 yet.");
                }

                UserManager<Auth0User> userManager = Engine.Services.Resolve<UserManager<Auth0User>>();
                SignInManager<Auth0User> signInManager = Engine.Services.Resolve<SignInManager<Auth0User>>();
                Auth0User user = await userManager.FindByIdAsync(id);
                string token = await userManager.GeneratePasswordResetTokenAsync(user);
                IdentityResult result = await userManager.ResetPasswordAsync(user, token, password);
                if (result.Succeeded)
                {
                    user.UserProfile.AddUserNote(new UserNote()
                    {
                        CreatedBy = User.GetLocalUserId(),
                        CreatedOn = DateTime.UtcNow,
                        Note = $"User password reset via admin panel by {User.Identity.Name}."
                    });
                    await userManager.UpdateAsync(user);
                    await _logService.AddLogAsync<Auth0UsersController>($"The password has been reset by an admin for user with Id: {id}", type: LogType.Success);
                    return new Response(true, $"The user's password has been reset.");
                }
                else
                {
                    string error = "";
                    foreach (IdentityError err in result.Errors)
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
    }
}
