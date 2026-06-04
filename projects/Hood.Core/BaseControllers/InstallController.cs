using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hood.Contexts;
using Hood.Core;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Hood.BaseControllers
{
    public class InstallController : Controller
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IConfiguration _config;

        public InstallController(
            IHostApplicationLifetime applicationLifetime,
            IConfiguration config
        )
        {
            _applicationLifetime = applicationLifetime;
            _config = config;
        }

        [HttpGet]
        [Route("/install")]
        public IActionResult Install()
        {
            if (Engine.Services.Installed)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new InstallModel { Email = Engine.Configuration?.SuperAdminEmail });
        }

        /// <summary>
        /// First-run setup: creates the administrator with the supplied credentials, grants the
        /// owner roles, seeds Hood (version, site owner, media directories, default settings), then
        /// restarts the host so it boots cleanly with <see cref="IHoodServiceProvider.Installed"/> true.
        /// </summary>
        [HttpPost]
        [Route("/install")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Install(InstallModel model)
        {
            if (Engine.Services.Installed)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // The chosen administrator email becomes the site owner for this initialisation run,
                // so the seed's GetSiteAdmin resolves to the account we are about to create.
                if (Engine.Configuration != null)
                {
                    Engine.Configuration.SuperAdminEmail = model.Email;
                }

                IPasswordAccountRepository accounts =
                    Engine.Services.Resolve<IPasswordAccountRepository>();

                // Ensure all Hood system roles exist.
                await accounts.SetupRolesAsync();

                // Create the administrator with the chosen credentials (unless one already exists).
                ApplicationUser admin = await accounts.GetUserByEmailAsync(model.Email);
                if (admin == null)
                {
                    // ApplicationUser and UserProfile share the AspNetUsers table (table-splitting),
                    // so both carry the same primary key.
                    string adminId = Guid.NewGuid().ToString();
                    admin = new ApplicationUser
                    {
                        Id = adminId,
                        UserName = model.Email,
                        Email = model.Email,
                        EmailConfirmed = true,
                        Active = true,
                        CreatedOn = DateTime.UtcNow,
                        LastLogOn = DateTime.UtcNow,
                        LastLoginIP = "127.0.0.1",
                        LastLoginLocation = "Setup",
                        UserProfile = new UserProfile
                        {
                            Id = adminId,
                            Email = model.Email,
                            UserName = model.Email,
                            FirstName = "Website",
                            LastName = "Administrator",
                            JobTitle = "Website Administrator",
                            Anonymous = false,
                        },
                    };

                    IdentityResult created = await accounts.CreateAsync(admin, model.Password);
                    if (!created.Succeeded)
                    {
                        foreach (IdentityError error in created.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }

                // Grant the administrator the owner-level roles.
                List<IdentityRole> ownerRoles = new List<IdentityRole>();
                foreach (string role in new[] { "SuperUser", "Admin" })
                {
                    IdentityRole roleObject =
                        await accounts.GetRoleAsync(role) ?? await accounts.CreateRoleAsync(role);
                    ownerRoles.Add(roleObject);
                }
                await accounts.AddUserToRolesAsync(admin, ownerRoles.ToArray());

                // Seed Hood: version stamp, site owner option, media directories, default settings.
                HoodDbContext hoodDb = Engine.Services.Resolve<HoodDbContext>();
                IdentityContext identity = Engine.Services.Resolve<IdentityContext>();
                await hoodDb.Seed(identity);

                // Restart on a short delay so this response can flush first. The container's restart
                // policy brings the host straight back up, now booting into an installed state.
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1500);
                    _applicationLifetime.StopApplication();
                });

                return RedirectToAction(nameof(Initialized));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Setup failed: " + ex.Message);
                return View(model);
            }
        }

        [Route("/install/ready")]
        public IActionResult Initialized()
        {
            return View();
        }
    }
}
