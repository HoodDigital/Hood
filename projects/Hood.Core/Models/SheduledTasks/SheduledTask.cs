using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Hood.Models
{
    public class ScheduledTask
    {

        /// <summary>
        /// The <see cref="System.Type">System.Type</see> to use for this tasks execution. There can only be one of each type. 
        /// </summary>
        [Display(Name = "Type", Description = "The type of IScheduledTask class to execute for this task.")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [Display(Name = "Name", Description = "The name of this scheduled task.")]
        public string Name { get; set; }

        /// <summary>
        /// How often to run this task in seconds.
        /// </summary>
        [Display(Name = "Interval", Description = "How often to run this task in seconds.")]
        public int Interval { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether a task is enabled
        /// </summary>
        [Display(Name = "Enabled?", Description = "Is this task enabled, when disabled the task will not run.")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Whether or not an error should cause the task to fail.
        /// </summary>
        [Display(Name = "Fail On Error?", Description = "Whether or not an error should cause the task to fail.")]
        public bool FailOnError { get; set; }

        /// <summary>
        /// The last time the task was sucessfully started.
        /// </summary>
        [Display(Name = "Latest Start", Description = "The last time the task was sucessfully started.")]
        public DateTime? LatestStart { get; set; }
        /// <summary>
        /// The time when the task last ended.
        /// </summary>
        [Display(Name = "Latest End", Description = "The time when the task last ended.")]
        public DateTime? LatestEnd { get; set; }
        /// <summary>
        /// The last time the task was sucessfully finished.
        /// </summary>
        [Display(Name = "Latest Success", Description = "The last time the task was sucessfully finished.")]
        public DateTime? LatestSuccess { get; set; }
    }
}
