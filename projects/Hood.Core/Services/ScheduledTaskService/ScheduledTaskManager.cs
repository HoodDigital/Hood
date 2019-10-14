using Hood.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hood.Services
{
    public partial class ScheduledTaskManager
    {
        public const string Path = "api/tasks/run-task";
        private readonly List<ScheduledTaskThread> _threads = new List<ScheduledTaskThread>();

        public static ScheduledTaskManager Instance { get; } = new ScheduledTaskManager();

        public IList<ScheduledTaskThread> TaskThreads => new ReadOnlyCollection<ScheduledTaskThread>(_threads);

        private ScheduledTaskManager()
        {
        }

        public void Initialize()
        {
            _threads.Clear();

            List<Models.ScheduledTask> scheduleTasks = Engine.Settings.ScheduledTasks.Tasks
                .OrderBy(x => x.Interval)
                .ToList();

            DateTime? systemStartTime = DateTime.UtcNow.Date
                .AddHours(Engine.Settings.ScheduledTasks.FixedStartTimeHours)
                .AddMinutes(Engine.Settings.ScheduledTasks.FixedStartTimeMinutes);

            foreach (Models.ScheduledTask scheduleTask in scheduleTasks)
            {
                //create a thread
                ScheduledTaskThread thread = new ScheduledTaskThread
                {
                    Seconds = scheduleTask.Interval,
                    FixedStartTime = !scheduleTask.FixedTime ? null : systemStartTime
                };
                thread.AddTask(scheduleTask);
                _threads.Add(thread);
            }

            List<Models.ScheduledTask> notRunTasks = scheduleTasks
                .Where(x => !x.LatestStart.HasValue || x.LatestStart.Value.AddSeconds(x.Interval) < DateTime.UtcNow)
                .ToList();

            if (notRunTasks.Any())
            {
                ScheduledTaskThread taskThread = new ScheduledTaskThread
                {
                    RunOnlyOnce = true,
                    Seconds = 0
                };
                foreach (Models.ScheduledTask scheduleTask in notRunTasks)
                {
                    taskThread.AddTask(scheduleTask);
                }
                _threads.Add(taskThread);
            }
        }

        /// <summary>
        /// Starts the task manager
        /// </summary>
        public void Start()
        {
            foreach (ScheduledTaskThread taskThread in _threads)
            {
                taskThread.InitTimer();
            }
        }

        /// <summary>
        /// Stops the task manager
        /// </summary>
        public void Stop()
        {
            foreach (ScheduledTaskThread taskThread in _threads)
            {
                taskThread.Dispose();
            }
        }
    }
}
