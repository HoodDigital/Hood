using Hood.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Hood.Core.ScheduledTasks
{
    public partial class KeepAliveTask : IScheduledTask
    {
        public KeepAliveTask()
        {
        }

        /// <summary>
        /// Executes the keep alive task
        /// </summary>
        public void Execute()
        {
            var url = Engine.Url + "keepalive/index";
            using (var wc = new WebClient())
            {
                wc.DownloadString(url);
            }
        }
    }
}
