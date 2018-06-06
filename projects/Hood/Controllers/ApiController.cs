using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hood.Models;
using Hood.Services;
using Hood.ViewModels;
using Hood.Core;
using Microsoft.AspNetCore.Http;
using Hood.Enums;
using Hood.BaseTypes;
using Hood.Extensions;
using Hood.Infrastructure;
using Hood.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

namespace Hood.Controllers
{
    [Authorize(Roles= "Admin,Api")]
    public class ApiController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly UrlEncoder _urlEncoder;
        private readonly IAccountRepository _auth;
        private readonly IMediaManager<MediaObject> _media;
        private readonly IBillingService _billing;
        private readonly HoodDbContext _db;

        public ApiController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          IEmailSender emailSender,
          ILogger<ManageController> logger,
          UrlEncoder urlEncoder,
          IAccountRepository auth,
          ILoggerFactory loggerFactory,
          IBillingService billing,
          HoodDbContext db,
          IMediaManager<MediaObject> media)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _urlEncoder = urlEncoder;
            _auth = auth;
            _media = media;
            _billing = billing;
            _db = db;
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
