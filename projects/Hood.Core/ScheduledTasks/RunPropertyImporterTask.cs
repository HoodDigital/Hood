using Hood.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Hood.Core.ScheduledTasks
{
    public partial class RunPropertyImporterTask : IScheduledTask
    {
        public RunPropertyImporterTask()
        {
        }

        /// <summary>
        /// Executes the task
        /// </summary>
        public void Execute()
        {
            var url = Engine.Url + "admin/property/import/blm/trigger";
            using (var wc = new WebClient())
            {
                wc.Headers.Add("Auth", Engine.Settings.Property.TriggerAuthKey);
                wc.DownloadString(url);
            }
        }
    }
}
