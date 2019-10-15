using Hood.Core;
using Hood.Core.Interfaces;
using Hood.Models;
using System;
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
                {
                    _enabled = ScheduledTask?.Enabled;
                }

                return _enabled.HasValue && _enabled.Value;
            }
            set => _enabled = value;
        }

        private async Task ExecuteTaskAsync()
        {
            if (!Enabled)
            {
                return;
            }

            Type type = Type.GetType(ScheduledTask.Type);
            if (type == null)
            {
                throw new Exception($"The scheduled task ({ScheduledTask.Type}) could not be loaded.");
            }

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

            IScheduledTask task = instance as IScheduledTask;
            if (task == null)
            {
                return;
            }

            ScheduledTaskSettings settings = Engine.Settings.ScheduledTasks;
            ScheduledTask.LatestStart = DateTime.UtcNow;
            //update appropriate datetime properties
            settings.Update(ScheduledTask);
            Engine.Settings.Set(settings);
            try
            {
                task.Execute();
                ScheduledTask.LatestEnd = ScheduledTask.LatestSuccess = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                ScheduledTask.LatestEnd = DateTime.UtcNow;

                ILogService logService = Engine.Services.Resolve<ILogService>();
                await logService.AddExceptionAsync<TaskExecutor>("An error occured during a scheduled task.", ex);
            }
            //update appropriate datetime properties
            settings.Update(ScheduledTask);
            Engine.Settings.Set(settings);
        }

        protected virtual bool IsRunning(ScheduledTask task)
        {
            if (!task.LatestStart.HasValue && !task.LatestEnd.HasValue)
            {
                return false;
            }

            DateTime latest = task.LatestStart ?? DateTime.UtcNow;

            if (task.LatestEnd.HasValue && latest < task.LatestEnd)
            {
                return false;
            }

            if (latest.AddSeconds(task.Interval) <= DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public async Task ExecuteAsync()
        {
            if (ScheduledTask == null || !Enabled)
            {
                return;
            }

            if (IsRunning(ScheduledTask))
            {
                return;
            }

            if (ScheduledTask.LatestSuccess.HasValue && (DateTime.UtcNow - ScheduledTask.LatestSuccess).Value.TotalSeconds < ScheduledTask.Interval)
            {
                return;
            }

            try
            {
                await ExecuteTaskAsync();
            }
            catch (Exception ex)
            {
                ScheduledTask.Enabled = !ScheduledTask.FailOnError;
                ScheduledTask.LatestEnd = DateTime.UtcNow;

                ScheduledTaskSettings settings = Engine.Settings.ScheduledTasks;
                settings.Update(ScheduledTask);
                Engine.Settings.Set<ScheduledTaskSettings>(settings);

                ILogService logService = Engine.Services.Resolve<ILogService>();
                await logService.AddExceptionAsync<TaskExecutor>("An error occured during a scheduled task.", ex);
            }
        }
    }
}
