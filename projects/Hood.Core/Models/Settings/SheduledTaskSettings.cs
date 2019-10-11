using Hood.BaseTypes;
using Hood.Core.ScheduledTasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hood.Models
{
    public class SheduledTaskSettings : SaveableModel
    {
        public SheduledTask[] Tasks { get; set; }

        [JsonIgnore]
        public List<SheduledTask> System
        {
            get
            {
                DateTime startTime = DateTime.Now.Date.AddHours(3);
                return new List<SheduledTask>()
                {
                    new SheduledTask() { Enabled = true, FailOnError = false, Interval = 600, Type = nameof(KeepAliveTask), Name = "Keep Alive" },
                    new SheduledTask() { Enabled = true, FailOnError = false, Interval = 86400, Type = nameof(RunPropertyImporterTask), Name = "Auto Property Import", LatestStart = startTime, LatestEnd = startTime  },
                };
            }
        }

        public SheduledTaskSettings()
        {
            Tasks = System.ToArray();
        }

        internal void CheckTasks()
        {
            List<SheduledTask> safePendingList = Tasks.ToList();
            safePendingList.RemoveAll(task => System.Any(st => task.Type == st.Type));
            foreach (SheduledTask systemTask in System)
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