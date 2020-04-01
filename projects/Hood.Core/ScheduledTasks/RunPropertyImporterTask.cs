using Hood.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        public async Task ExecuteAsync()
        {
            try
            {
                var url = Engine.Url + "admin/property/import/blm/trigger";
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("Auth", Engine.Settings.Property.TriggerAuthKey);
                    wc.DownloadString(url);
                }
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while processing the RunPropertyImporterTask, see InnerException for more information.", ex);
            }
        }
    }
}
