using Hood.Core;
using Hood.Core.Interfaces;
using Hood.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hood.Services
{
    public partial class TaskExecutor
    {
        private bool? _enabled;

        public TaskExecutor(ScheduledTask task)
        {
            ScheduledTask = task;
        }

        public ScheduledTask ScheduledTask { get; }

        public bool Enabled
        {
            get
            {
                if (!_enabled.HasValue)
                    _enabled = ScheduledTask?.Enabled;

                return _enabled.HasValue && _enabled.Value;
            }
            set => _enabled = value;
        }

        private void ExecuteTask()
        {
            if (!Enabled)
                return;

            var type = Type.GetType(ScheduledTask.Type);
            if (type == null)
                throw new Exception($"The scheduled task ({ScheduledTask.Type}) could not be loaded.");

            object instance = null;
            try
            {
                instance = Engine.Services.Resolve(type);
            }
            catch
            {
                //try resolve
            }
            if (instance == null)
            {
                //not resolved
                instance = Engine.Services.ResolveUnregistered(type);
            }

            var task = instance as IScheduledTask;
            if (task == null)
                return;

            ScheduledTask.LatestStart = DateTime.UtcNow;
            //update appropriate datetime properties
            Engine.Settings.ScheduledTasks.Update(ScheduledTask);
            task.Execute();
            ScheduledTask.LatestEnd = ScheduledTask.LatestSuccess = DateTime.UtcNow;
            //update appropriate datetime properties
            Engine.Settings.ScheduledTasks.Update(ScheduledTask);
        }

        protected virtual bool IsRunning(ScheduledTask task)
        {
            if (!task.LatestStart.HasValue && !task.LatestEnd.HasValue)
                return false;

            var latest = task.LatestStart ?? DateTime.UtcNow;

            if (task.LatestEnd.HasValue && latest < task.LatestEnd)
                return false;

            if (latest.AddSeconds(task.Interval) <= DateTime.UtcNow)
                return false;

            return true;
        }

        public async Task ExecuteAsync()
        {
            if (ScheduledTask == null || !Enabled)
                return;

            if (IsRunning(ScheduledTask))
                return;

            if (ScheduledTask.LatestEnd.HasValue && (DateTime.UtcNow - ScheduledTask.LatestEnd).Value.TotalSeconds < ScheduledTask.Interval)
                return;

            try
            {
                ExecuteTask();
            }
            catch (Exception ex)
            {
                ScheduledTask.Enabled = !ScheduledTask.FailOnError;
                ScheduledTask.LatestEnd = DateTime.UtcNow;
                Engine.Settings.ScheduledTasks.Update(ScheduledTask);

                var logService = Engine.Services.Resolve<ILogService>();
                await logService.AddExceptionAsync<TaskExecutor>("An error occured during a scheduled task.", ex);
            }
        }
    }
}
