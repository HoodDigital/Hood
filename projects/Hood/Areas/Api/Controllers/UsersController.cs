using Hood.Controllers;
using Hood.Enums;
using Hood.Extensions;
using Hood.Filters;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Hood.Areas.Api.Controllers
{
    [Area("Api")]
    public class UsersController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        public UsersController()
            : base()
        {
        }

        [Authorize]
        [ApiAuthorize(AccessLevel.Public)]
        [Route("api/roles/invite/")]
        public async Task<IActionResult> InviteRoleAsync(string role)
        {
            // add the user to the role
            var user = await _userManager.GetUserAsync(User);

            if (Roles.System.Contains(role))
                return View("Api", new ApiViewModel()
                {
                    SaveMessage = "You cannot be added to the '" + role.ToSentenceCase() + "' role, this cannot be done via the API.",
                    MessageType = AlertType.Warning,
                    Title = "Could not add you to the '" + role.ToSentenceCase() + "' role.",
                    Details = "You cannot be added to the '" + role.ToSentenceCase() + "' role , this cannot be done via the API.",
                });

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
                        SaveMessage = "You have not been added to the '" + role.ToSentenceCase() + "' role, you are already in that role.",
                        MessageType = AlertType.Warning,
                        Title = "Could not add you to '" + role.ToSentenceCase() + "' role.",
                        Details = "You have not been added to the '" + role.ToSentenceCase() + "' role, you are already in that role.",
                    });
                }
            }
            else
            {
                return View("Api", new ApiViewModel()
                {
                    SaveMessage = "You have not been added to the '" + role.ToSentenceCase() + "' role, the role does not exist.",
                    MessageType = AlertType.Danger,
                    Title = "Could not add you to '" + role.ToSentenceCase() + "' role.",
                    Details = "You have not been added to the " + role.ToSentenceCase() + " role, the role does not exist.",
                });
            }

            // Reset the user's login to refresh the roles.
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);

            ApiViewModel model = new ApiViewModel()
            {
                SaveMessage = "You have been added to the '" + role.ToSentenceCase() + "' role, you will be able to access the new features right away.",
                MessageType = AlertType.Success,
                Title = "Added to '" + role.ToSentenceCase() + "' role.",
                Details = "You have been added to the " + role.ToSentenceCase() + "' role, you will be able to access the new features right away.",
            };

            return View("Api", model);
        }

        [HttpGet]
        [ApiAuthorize(AccessLevel.Public)]
        [Route("api/test/get/")]
        public IActionResult TestGetEndpoint(string variable)
        {
            return Json(new { success = true, result = new { input = variable } });
        }

        [HttpPost]
        [ApiAuthorize]
        [Route("api/test/post/")]
        public IActionResult TestPostEndpoint(string variable)
        {
            return Json(new { success = true, result = new { input = variable } });
        }

    }
}
