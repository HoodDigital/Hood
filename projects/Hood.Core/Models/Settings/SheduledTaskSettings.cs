using Hood.BaseTypes;
using Hood.Core.ScheduledTasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
                return new List<ScheduledTask>()
                {
                    new ScheduledTask() {
                        Name = "Keep Alive",
                        Interval = 600,
                        Type = typeof(KeepAliveTask).ToString(),
                        Enabled = true,
                        FailOnError = false
                    },
                    new ScheduledTask() {
                        Name = "Auto Property Import",
                        Interval = 86400,
                        Type = typeof(RunPropertyImporterTask).ToString(),
                        FixedTime = true,
                        Enabled = true,
                        FailOnError = false
                    }
                };
            }
        }

        [Display(Name = "Start Time (Hours)")]
        public int FixedStartTimeHours { get; set; }
        [Display(Name = "Start Time (Minutes)")]
        public int FixedStartTimeMinutes { get; set; }

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
                    t.FixedTime = task.FixedTime;
                    t.FailOnError = task.FailOnError;
                    t.LatestEnd = task.LatestEnd;
                    t.LatestStart = task.LatestStart;
                    t.LatestSuccess = task.LatestSuccess;
                }
            });
            Tasks = tasks.ToArray();
        }

        public ScheduledTaskSettings()
        {
            Tasks = System.ToArray();
            FixedStartTimeHours = 3;
            FixedStartTimeMinutes = 0;
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