using Hood.Core;
using Hood.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Controllers
{
    [Area("Api")]
    public partial class ScheduleTaskController : Controller
    {
        [HttpPost]
        [Route(ScheduledTaskManager.Path)]
        public virtual async System.Threading.Tasks.Task<IActionResult> RunTaskAsync(string type)
        {
            var scheduledTask = Engine.Settings.ScheduledTasks.GetByType(type);
            if (scheduledTask == null)
                return NoContent();

            var task = new TaskExecutor(scheduledTask);
            await task.ExecuteAsync();
            return NoContent();
        }
    }
}
