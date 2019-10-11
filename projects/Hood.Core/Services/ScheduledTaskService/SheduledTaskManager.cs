using Hood.Core;
using Hood.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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

            var scheduleTasks = Engine.Settings.ScheduledTasks.Tasks
                .OrderBy(x => x.Interval)
                .ToList();

            foreach (var scheduleTaskGrouped in scheduleTasks.GroupBy(x => x.Interval))
            {
                //create a thread
                var thread = new ScheduledTaskThread
                {
                    Seconds = scheduleTaskGrouped.Key
                };
                foreach (var scheduleTask in scheduleTaskGrouped)
                {
                    thread.AddTask(scheduleTask);
                }
                _threads.Add(thread);
            }

            var notRunTasks = scheduleTasks
                .Where(x => x.Interval >= 1800)
                .Where(x => !x.LatestStart.HasValue || x.LatestStart.Value.AddSeconds(x.Interval) < DateTime.UtcNow)
                .ToList();

            if (notRunTasks.Any())
            {
                var taskThread = new ScheduledTaskThread
                {
                    RunOnlyOnce = true,
                    Seconds = 60 * 5 
                };
                foreach (var scheduleTask in notRunTasks)
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
            foreach (var taskThread in _threads)
            {
                taskThread.InitTimer();
            }
        }

        /// <summary>
        /// Stops the task manager
        /// </summary>
        public void Stop()
        {
            foreach (var taskThread in _threads)
            {
                taskThread.Dispose();
            }
        }
    }
}
