using Hood.Controllers;
using Hood.Extensions;
using Hood.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
    public class LogsController : BaseController
    {
        public LogsController()
            : base()
        {
        }

        public async Task<IActionResult> Index(LogListModel model)
        {
            return await Show(model);
        }
        public async Task<IActionResult> Clear()
        {
            try
            {
                _db.Database.SetCommandTimeout(new TimeSpan(0, 10, 0));
                await _db.Database.ExecuteSqlRawAsync("DELETE FROM HoodLogs");
                _db.Database.SetCommandTimeout(new TimeSpan(0, 0, 30));
                SaveMessage = "Logs have been cleared.";
                MessageType = Enums.AlertType.Success;
            } catch (Exception ex)
            {
                SaveMessage = "Error clearing the site logs.";
                MessageType = Enums.AlertType.Danger;
                await _logService.AddExceptionAsync<LogsController>(SaveMessage, ex);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Show(LogListModel model)
        {
            var logs = _db.Logs
                .Include(l => l.User).Where(l => l.Time > DateTime.MinValue);

            if (model.Source.IsSet())
            {
                logs = logs.Where(l => l.Source == model.Source);
            }

            if (model.LogType.HasValue)
            {
                logs = logs.Where(l => l.Type == model.LogType);
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
