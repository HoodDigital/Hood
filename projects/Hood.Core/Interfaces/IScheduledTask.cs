using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Core.Interfaces
{
    public interface IScheduledTask
    {
        /// <summary>
        /// Executes the task, this will be called by task handlers.
        /// </summary>
        void Execute();
    }
}
