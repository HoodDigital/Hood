using Hood.BaseTypes;
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

namespace Hood.Areas.Api.Controllers
{
    [Area("Api")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAccountRepository _auth;
        private readonly IContentRepository _content;
        private readonly HoodDbContext _db;
        private readonly IEmailSender _email;
        private readonly ISettingsRepository _settings;
        private readonly IBillingService _billing;

        public UsersController(
            HoodDbContext db,
            IAccountRepository auth,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender email,
            ISettingsRepository site,
            IBillingService billing,
            IContentRepository content)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _auth = auth;
            _db = db;
            _email = email;
            _settings = site;
            _content = content;
            _billing = billing;
        }

        [Route("api/roles/invite/")]
        [Authorize]
        [ApiAuthorize]
        public async Task<IActionResult> InviteRoleAsync(string role)
        {
            // add the user to the role
            var user = await _userManager.GetUserAsync(User);

            if (await _roleManager.RoleExistsAsync(role))
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    return View("Api", new ApiViewModel()
                    {
                        SaveMessage = "You have not been added to the " + role.ToSentenceCase() + ", you are already in that role.",
                        MessageType = AlertType.Warning,
                        Title = "Could not add you to " + role.ToSentenceCase(),
                        Details = "You have not been added to the " + role.ToSentenceCase() + ", you are already in that role.",
                    });
                }
            }
            else
            {
                return View("Api", new ApiViewModel()
                {
                    SaveMessage = "You have not been added to the " + role.ToSentenceCase() + ", the role does not exist.",
                    MessageType = AlertType.Danger,
                    Title = "Could not add you to " + role.ToSentenceCase(),
                    Details = "You have not been added to the " + role.ToSentenceCase() + ", the role does not exist.",
                });
            }

            // Reset the user's login to refresh the roles.
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);

            ApiViewModel model = new ApiViewModel()
            {
                SaveMessage = "You have been added to the " + role.ToSentenceCase() + ", you will be able to access the new features right away.",
                MessageType = AlertType.Success,
                Title = "Added to " + role.ToSentenceCase(),
                Details = "You have been added to the " + role.ToSentenceCase() + ", you will be able to access the new features right away.",
            };

            return View("Api", model);
        }
    }
}
