
using Hood.BaseControllers;
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
    public abstract class UsersController : BaseController
    {
        protected readonly IPasswordAccountRepository _account;
        public UsersController()
            : base()
        {
            _account = Engine.Services.Resolve<IPasswordAccountRepository>();
        }

        #region User List 

        [Route("admin/users/")]
        public virtual async Task<IActionResult> Index(UserListModel<UserProfileView<IdentityRole>> model)
        {
            return await List(model, "Index");
        }

        [HttpGet]
        [Route("admin/users/list/")]
        public virtual async Task<IActionResult> List(UserListModel<UserProfileView<IdentityRole>> model, string viewName = "_List_Users")
        {
            model = await _account.GetUserProfileViewsAsync(model);
            return View(viewName, model);
        }

        #endregion

        #region Edit
        [Route("admin/users/{id}/edit/")]
        public virtual async Task<IActionResult> Edit(string id)
        {
            UserProfileView<IdentityRole> model = await _account.GetUserProfileViewById(id);
            var roles = await _account.GetRolesAsync(new RoleListModel<IdentityRole> { PageIndex = 0, PageSize = int.MaxValue });
            model.AllRoles = roles.List;
            return View(model);
        }

        [Route("admin/users/{id}/edit/")]
        [HttpPost]
        public virtual async Task<IActionResult> Edit(UserProfileView<IdentityRole> model)
        {
            ApplicationUser modelToUpdate = await _account.GetUserByIdAsync(model.Id);
            try
            {
                string email = modelToUpdate.Email;
                if (model.Email != email)
                {
                    await _account.SetEmailAsync(modelToUpdate, model.Email);
                }

                string phoneNumber = modelToUpdate.PhoneNumber;
                if (model.PhoneNumber != phoneNumber)
                {
                    await _account.SetPhoneNumberAsync(modelToUpdate, model.PhoneNumber);
                }

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

            var roles = await _account.GetRolesAsync(new RoleListModel<IdentityRole> { PageIndex = 0, PageSize = int.MaxValue });
            model.AllRoles = roles.List;
            return View(model);
        }

        #endregion

        #region Create
        [Route("admin/users/create/")]
        public virtual IActionResult Create()
        {
            return View();
        }

        [Route("admin/users/create/")]
        [HttpPost]
        public virtual async Task<Response> Create(AdminCreateUserViewModel model)
        {
            try
            {
                ApplicationUser user = new ApplicationUser
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
                IdentityResult result = await _account.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return new Response(result.Errors);
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
                        ApplicationUser deleteUser = await _account.GetUserByEmailAsync(model.Email);
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
                ApplicationUser user = await _account.GetUserByIdAsync(id);
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
                ApplicationUser user = await _account.GetUserByIdAsync(id);
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
            return View("_Inline_Notes", model);
        }

        [Route("admin/users/{id}/notes/add/")]
        [HttpPost]
        public virtual async Task<Response> AddNote(string id, string note)
        {
            try
            {
                ApplicationUser user = await _account.GetUserByIdAsync(id);
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
                ApplicationUser user = await _account.GetUserByIdAsync(id);
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
        public virtual async Task<IActionResult> Roles(RoleListModel<IdentityRole> model)
        {
            return await ListRoles(model, "Roles");
        }

        [HttpGet]
        [Route("admin/roles/list/")]
        public virtual async Task<IActionResult> ListRoles(RoleListModel<IdentityRole> model, string viewName = "_List_Roles")
        {
            model = await _account.GetRolesAsync(model);
            return View(viewName, model);
        }

        // [Route("admin/roles/create/")]
        // public virtual IActionResult CreateRole()
        // {
        //     return View();
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
        //         return await ErrorResponseAsync<UsersController>($"Error creating a role via the admin panel.", ex);
        //     }
        // }

        [Route("admin/roles/edit/")]
        public virtual async Task<IActionResult> EditRole(string roleName)
        {
            var role = await _account.GetRoleAsync(roleName);
            return View(role);
        }

        [Route("admin/roles/{id}/delete/")]
        [HttpPost]
        public virtual async Task<Response> DeleteRole(string id)
        {
            try
            {
                IdentityRole role = await _account.GetRoleAsync(id);
                if (role == null)
                {
                    throw new Exception($"The role Id {id} could not be found, therefore could not be deleted.");
                }

                await _account.DeleteRoleAsync(id);
                await _logService.AddLogAsync<UsersController>($"The role ({role.Name}) has been deleted via the admin area by {User.Identity.Name}", type: LogType.Warning);
                return new Response(true, "Deleted successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error deleting a role via the admin panel.", ex);
            }
        }
        #endregion

        #region User Roles
        [Route("admin/users/{id}/get-roles/")]
        [HttpGet]
        public virtual async Task<JsonResult> GetRoles(string id)
        {
            ApplicationUser user = await _account.GetUserByIdAsync(id);
            IList<IdentityRole> roles = await _account.GetRolesForUser(user);
            return Json(new { success = true, roles });
        }
        [Route("admin/users/{id}/set-role/")]
        [HttpPost]
        public virtual async Task<Response> SetRole(string id, string role, bool add)
        {
            try
            {
                ApplicationUser user = await _account.GetUserByIdAsync(id);
                IdentityRole IdentityRole = await _account.GetRoleAsync(role);
                if (!await _account.RoleExistsAsync(role))
                {
                    await _account.CreateRoleAsync(role);
                }

                var roles = new List<IdentityRole>() {
                    IdentityRole
                };

                if (add)
                {
                    await _account.AddUserToRolesAsync(user, roles.ToArray());
                }
                else
                {
                    await _account.RemoveUserFromRolesAsync(user, roles.ToArray());
                }

                if (add)
                {
                    return await SuccessResponseAsync<UsersController>($"The user has been added the {role} role.");
                }
                else
                {
                    return await SuccessResponseAsync<UsersController>($"The user has been removed from the {role} role.");
                }
            }
            catch (Exception ex)
            {
                if (add)
                {
                    return await ErrorResponseAsync<UsersController>($"Error adding a user to a role via the admin panel.", ex);
                }
                else
                {
                    return await ErrorResponseAsync<UsersController>($"Error removing a user from a role via the admin panel.", ex);
                }
            }
        }
        #endregion

        #region Impersonation
        public virtual async Task<IActionResult> Impersonate(string id)
        {
            try
            {
                if (!id.IsSet())
                {
                    throw new Exception("Cannot impersonate a user with no Id!");
                }

                SignInManager<ApplicationUser> signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
                ApplicationUser impersonatedUser = await _account.GetUserByIdAsync(id);
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
                await _logService.AddExceptionAsync<UsersController>(SaveMessage, ex);
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

                ApplicationUser originalUser = await _account.GetUserByIdAsync(originalUserId);

                SignInManager<ApplicationUser> signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
                await signInManager.SignOutAsync();
                await signInManager.SignInAsync(originalUser, isPersistent: true);

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
        public virtual async Task<Response> ResetPassword(string id, string password)
        {
            try
            {
                UserManager<ApplicationUser> userManager = Engine.Services.Resolve<UserManager<ApplicationUser>>();
                SignInManager<ApplicationUser> signInManager = Engine.Services.Resolve<SignInManager<ApplicationUser>>();
                ApplicationUser user = await userManager.FindByIdAsync(id);
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
                    await _logService.AddLogAsync<UsersController>($"The password has been reset by an admin for user with Id: {id}", type: LogType.Success);
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
