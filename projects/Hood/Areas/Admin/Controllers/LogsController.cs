using Hood.Extensions;
using Hood.Models;
using Hood.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860
namespace Hood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Editor")]
    public class LogsController : Controller
    {
        private readonly HoodDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPropertyRepository _property;
        private readonly ISettingsRepository _settings;
        private readonly PropertySettings _propertySettings;
        private readonly IHostingEnvironment _env;
        private readonly IBillingService _billing;
        private readonly IMediaManager<MediaObject> _media;
        private readonly IAddressService _address;

        public LogsController(
            IPropertyRepository property,
            UserManager<ApplicationUser> userManager,
            ISettingsRepository settings,
            IMediaManager<MediaObject> media,
            IBillingService billing,
            IHostingEnvironment env,
            HoodDbContext db,
            IAddressService address)
        {
            _userManager = userManager;
            _property = property;
            _settings = settings;
            _billing = billing;
            _env = env;
            _media = media;
            _propertySettings = _settings.GetPropertySettings();
            _address = address;
            _db = db;
        }

        public async Task<IActionResult> Index(LogListModel model)
        {
            return await Show(model);
        }

        public async Task<IActionResult> Show(LogListModel model)
        {
            var logs = _db.Logs
                .Include(l => l.User).Where(l => l.Time > DateTime.MinValue);

            if (model.EntityId.IsSet())
            {
                //logs = logs.Where(l => l.QuestionWidgetId == model.QuestionWidgetId);
            }

            if (model.UserId.IsSet())
            {
                logs = logs.Where(l => l.UserId == model.UserId);
            }

            switch (model.Order)
            {
                case "date+desc":
                default:
                    logs = logs.OrderByDescending(l => l.Time);
                    break;
                case "date":
                    logs = logs.OrderBy(l => l.Time);
                    break;
                case "user":
                    logs = logs.OrderByDescending(l => l.User.Email);
                    break;
                case "user+desc":
                    logs = logs.OrderBy(l => l.User.Email);
                    break;
            }

            model.List = await logs.ToListAsync();
            return View(model);
        }

    }
}
