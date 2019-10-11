using Hood.BaseTypes;
using Hood.Core.ScheduledTasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public class ScheduledTaskSettings : SaveableModel
    {
        public ScheduledTask[] Tasks { get; set; }

        [JsonIgnore]
        public List<ScheduledTask> System
        {
            get
            {
                DateTime startTime = DateTime.Now.Date.AddHours(3);
                return new List<ScheduledTask>()
                {
                    new ScheduledTask() { Enabled = true, FailOnError = false, Interval = 600, Type = typeof(KeepAliveTask).ToString(), Name = "Keep Alive" },
                    new ScheduledTask() { Enabled = true, FailOnError = false, Interval = 86400, Type = typeof(RunPropertyImporterTask).ToString(), Name = "Auto Property Import", LatestStart = startTime, LatestEnd = startTime  },
                };
            }
        }

        public ScheduledTask GetByType(string type)
        {
            return Tasks.SingleOrDefault(t => t.Type == type);
        }
        public void Update(ScheduledTask task)
        {
            var tasks = Tasks.ToList();
            tasks.ForEach(t =>
            {
                if (t.Type == task.Type)
                {
                    t.LatestEnd = task.LatestEnd;
                    t.LatestStart = task.LatestStart;
                    t.LatestSuccess = task.LatestSuccess;
                }
            });
        }

        public ScheduledTaskSettings()
        {
            Tasks = System.ToArray();
        }

        internal void CheckTasks()
        {
            List<ScheduledTask> safePendingList = Tasks.ToList();
            safePendingList.RemoveAll(task => !System.Any(st => task.Type == st.Type));
            foreach (ScheduledTask systemTask in System)
            {
                if (!safePendingList.Any(task => task.Type == systemTask.Type))
                {
                    safePendingList.Add(systemTask);
                }
            }
            Tasks = safePendingList.ToArray();
        }
    }
}