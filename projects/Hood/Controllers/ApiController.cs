using Hood.Enums;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Hood.Controllers
{
    [Authorize(Roles= "Admin,Api")]
    public class ApiController : BaseController<HoodDbContext, ApplicationUser, IdentityRole>
    {
        private readonly UrlEncoder _urlEncoder;

        public ApiController(UrlEncoder urlEncoder)
        {
            _urlEncoder = urlEncoder;
        }

        public async Task<IActionResult> Index(EditorMessage? message = null)
        {
            // if the user doesnt have a key - create them one!
            var user = await _userManager.GetUserAsync(User);
            ApiKey model = await _db.ApiKeys
                .Include(f => f.User)
                .Include(f => f.Events)
                .FirstOrDefaultAsync(f => f.UserId == user.Id);

            model.AddEditorMessage(message);

            if (model == null)
            {
                await CreateNewKeyAsync(user);

                model = await _db.ApiKeys
                    .Include(f => f.User)
                    .Include(f => f.Events)
                    .FirstOrDefaultAsync(f => f.UserId == user.Id);

                model.AddEditorMessage(EditorMessage.KeyCreated);
            }
            return View(model);
        }

        public async Task<IActionResult> Roll()
        {
            // if the user doesnt have a key - create them one!
            var user = await _userManager.GetUserAsync(User);
            var model = await _db.ApiKeys
                .Include(f => f.User)
                .Include(f => f.Events)
                .FirstOrDefaultAsync(f => f.UserId == user.Id);

            if (model == null)
            {
                return RedirectToAction("Index", new { message = EditorMessage.NotFound });
            }

            var generator = new KeyGenerator(true, true, true, false);
            model.Key = generator.Generate(24);
            await _db.SaveChangesAsync();

            return View(model);
        }

        private async Task CreateNewKeyAsync(ApplicationUser user)
        {
            var generator = new KeyGenerator(true, true, true, false);
            var newKey = new ApiKey()
            {
                AccessLevel = AccessLevel.Restricted,
                Active = true,
                CreatedOn = DateTime.Now,
                Key = generator.Generate(24),
                Name = "User Key - " + user.UserName,
                UserId = user.Id
            };
            _db.ApiKeys.Add(newKey);
            await _db.SaveChangesAsync();
        }
    }
}
