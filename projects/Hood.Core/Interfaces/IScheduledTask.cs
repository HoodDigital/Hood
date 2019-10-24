using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hood.Core.Interfaces
{
    public interface IScheduledTask
    {
        /// <summary>
        /// Executes the task, this will be called by task handlers.
        /// </summary>
        Task ExecuteAsync();
    }
}
