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
                    new ScheduledTask() { Enabled = true, FailOnError = false, Interval = 600, Type = nameof(KeepAliveTask), Name = "Keep Alive" },
                    new ScheduledTask() { Enabled = true, FailOnError = false, Interval = 86400, Type = nameof(RunPropertyImporterTask), Name = "Auto Property Import", LatestStart = startTime, LatestEnd = startTime  },
                };
            }
        }

        public ScheduledTaskSettings()
        {
            Tasks = System.ToArray();
        }

        internal void CheckTasks()
        {
            List<ScheduledTask> safePendingList = Tasks.ToList();
            safePendingList.RemoveAll(task => System.Any(st => task.Type == st.Type));
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