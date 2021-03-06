﻿using Hood.Controllers;
using Hood.Core;
using Hood.Enums;
using Hood.Extensions;
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
        public async Task<IActionResult> Index(UserListModel model)
        {
            return await List(model, "Index");
        }

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
            ApplicationUser user = await _account.GetUserByIdAsync(model.Id);
            try
            {
                string email = user.Email;
                if (model.Email != email)
                {
                    IdentityResult setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                    if (!setEmailResult.Succeeded)
                    {
                        throw new Exception(setEmailResult.Errors.FirstOrDefault().Description);
                    }
                }

                string phoneNumber = user.PhoneNumber;
                if (model.PhoneNumber != phoneNumber)
                {
                    IdentityResult setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        model.Email = phoneNumber;
                        throw new Exception(setPhoneResult.Errors.FirstOrDefault().Description);
                    }
                }

                model.Notes = user.Notes;
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

        private async Task LoadAndCheckProfile(UserProfile model)
        {
            model.AllRoles = await _account.GetAllRolesAsync();
            model.AccessCodes = await _account.GetAccessCodesAsync(model.Id);
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
        public async Task<Response> Create(AdminCreateUserViewModel model)
        {
            try
            {
                ApplicationUser user = new ApplicationUser
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
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    return new Response(result.Errors);
                }

                if (model.CreateValidated)
                {
                    user.EmailConfirmed = true;
                    user.Active = true;
                    await _userManager.UpdateAsync(user);
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
                        message.AddParagraph("Name: <strong>" + user.ToFullName() + "</strong>");
                        message.AddParagraph("Username: <strong>" + model.Username + "</strong>");
                        message.AddParagraph("Password: <strong>" + model.Password + "</strong>");
                        message.AddCallToAction("Log in here", ControllerContext.HttpContext.GetSiteUrl() + "/account/login", "#32bc4e", "center");
                        await _emailSender.SendEmailAsync(message);
                    }
                    catch (Exception)
                    {
                        // roll back!
                        ApplicationUser deleteUser = await _userManager.FindByEmailAsync(model.Username);
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
        public async Task<Response> MarkEmailConfirmed(string id)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    throw new Exception($"The user Id {id} could not be found, therefore could not be confirmed.");
                }

                user.EmailConfirmed = true;
                if (!user.Active)
                {
                    user.Active = true;
                }
                user.AddUserNote(new UserNote()
                {
                    CreatedBy = User.GetUserId(),
                    CreatedOn = DateTime.Now,
                    Note = $"User email marked as confirmed via admin panel by {User.Identity.Name}."
                });
                await _userManager.UpdateAsync(user);

                await _logService.AddLogAsync<UsersController>($"The user account ({user.Email}) email confirmed via admin panel by {User.Identity.Name}", type: LogType.Warning);
                return new Response(true, "Email confirmed successfully.");
            }
            catch (Exception ex)
            {
                return await ErrorResponseAsync<UsersController>($"Error confirming email for user via the admin panel.", ex);
            }
        }
        //[Route("admin/users/{id}/confirm-phone/")]
        //public async Task<Response> MarkPhoneConfirmed(string id)
        //{
        //    try
        //    {
        //        ApplicationUser user = await _userManager.FindByIdAsync(id);
        //        if (user == null)
        //        {
        //            throw new Exception($"The user Id {id} could not be found, therefore could not be confirmed.");
        //        }

        //        user.PhoneNumberConfirmed = true;
        //        if (!user.Active)
        //        {
        //            user.Active = true;
        //        }
        //        user.AddUserNote(new UserNote()
        //        {
        //            CreatedBy = User.GetUserId(),
        //            CreatedOn = DateTime.Now,
        //            Note = $"User phone marked as confirmed via admin panel by {User.Identity.Name}."
        //        });
        //        await _userManager.UpdateAsync(user);

        //        await _logService.AddLogAsync<UsersController>($"The user account ({user.Email}) phone confirmed via admin panel by {User.Identity.Name}", type: LogType.Warning);
        //        return new Response(true, "Phone confirmed successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return await ErrorResponseAsync<UsersController>($"Error confirming phone for user via the admin panel.", ex);
        //    }
        //}
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
                ApplicationUser user = await _account.GetUserByIdAsync(id);
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
                ApplicationUser user = await _account.GetUserByIdAsync(id);
                List<UserNote> notes = user.Notes;
                UserNote note = notes.SingleOrDefault(n => n.Id == noteId);
                if (note != null && notes.Contains(note))
                {
                    notes.Remove(note);
                }

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

        #region Roles
        [Route("admin/users/{id}/get-roles/")]
        [HttpGet]
        public async Task<JsonResult> GetRoles(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            IList<string> roles = await _userManager.GetRolesAsync(user);
            return Json(new { success = true, roles });
        }
        [Route("admin/users/{id}/set-role/")]
        [HttpPost]
        public async Task<Response> SetRole(string id, string role, bool add)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByIdAsync(id);
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                IdentityResult result;

                if (add)
                {
                    result = await _userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    result = await _userManager.RemoveFromRoleAsync(user, role);
                }

                if (result.Succeeded)
                {
                    if (add)
                    {
                        return await SuccessResponseAsync<UsersController>($"The user has been added the {role} role.");
                    }
                    else
                    {
                        return await SuccessResponseAsync<UsersController>($"The user has been removed from the {role} role.");
                    }
                }
                else
                {
                    IdentityError error = result.Errors.FirstOrDefault();
                    if (error != null)
                    {
                        throw new Exception(error.Description);
                    }
                    throw new Exception("The user manager could not save the role update to the database.");
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
        public async Task<IActionResult> Impersonate(string id)
        {
            try
            {
                if (!id.IsSet())
                {
                    throw new Exception("Cannot impersonate a user with no Id!");
                }

                ApplicationUser impersonatedUser = await _userManager.FindByIdAsync(id);
                ClaimsPrincipal userPrincipal = await _signInManager.CreateUserPrincipalAsync(impersonatedUser);

                userPrincipal.Identities.First().AddClaim(new Claim("OriginalUserId", User.GetUserId()));
                userPrincipal.Identities.First().AddClaim(new Claim("IsImpersonating", "true"));

                impersonatedUser.AddUserNote(new UserNote()
                {
                    CreatedBy = User.GetUserId(),
                    CreatedOn = DateTime.Now,
                    Note = $"User {impersonatedUser.UserName} was impersonated by {User.Identity.Name}."
                });
                await _userManager.UpdateAsync(impersonatedUser);

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
                {
                    throw new Exception("You are not logged in.");
                }

                if (!User.IsImpersonating())
                {
                    throw new Exception("You are not impersonating.");
                }

                string originalUserId = User.FindFirst("OriginalUserId").Value;

                ApplicationUser originalUser = await _userManager.FindByIdAsync(originalUserId);

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
                string token = await _userManager.GeneratePasswordResetTokenAsync(user);
                IdentityResult result = await _userManager.ResetPasswordAsync(user, token, password);
                if (result.Succeeded)
                {
                    user.AddUserNote(new UserNote()
                    {
                        CreatedBy = User.GetUserId(),
                        CreatedOn = DateTime.Now,
                        Note = $"User password reset via admin panel by {User.Identity.Name}."
                    });
                    await _userManager.UpdateAsync(user);
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
