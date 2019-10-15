using Hood.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        public async Task ExecuteAsync()
        {
            // This task does not need to do anything, the server has been hit by the task executor, so it will keep the system alive anyway.
        }
    }
}
