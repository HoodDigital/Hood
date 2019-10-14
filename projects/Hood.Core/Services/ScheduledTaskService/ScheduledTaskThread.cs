using Hood.Core;
using Hood.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;

namespace Hood.Services
{
    public partial class ScheduledTaskThread : IDisposable
    {
        private Timer _timer;
        private bool _disposed;
        private readonly Dictionary<string, string> _tasks;
        public int Seconds { get; set; }

        public DateTime StartedUtc { get; private set; }

        public bool IsRunning { get; private set; }

        public int Interval
        {
            get
            {
                int interval = Seconds * 1000;
                if (interval <= 0)
                {
                    interval = int.MaxValue;
                }

                return interval;
            }
        }
        public bool RunOnlyOnce { get; set; }
        public DateTime? FixedStartTime { get; set; }

        internal ScheduledTaskThread()
        {
            _tasks = new Dictionary<string, string>();
            Seconds = 10 * 60;
        }

        private void Run()
        {
            if (Seconds <= 0)
            {
                return;
            }

            StartedUtc = DateTime.UtcNow;
            IsRunning = true;
            foreach (string type in _tasks.Values)
            {
                try
                {
                    NameValueCollection postData = new NameValueCollection
                    {
                        {"type", type}
                    };
                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add("Key", Engine.ApplicationKey);
                        if (Engine.Url != null)
                        {
                            client.UploadValues(Path.Combine(Engine.Url, ScheduledTaskManager.Path), postData);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Engine.Logs.AddExceptionAsync<ScheduledTaskManager>(ex.Message, ex);
                }

            }
            IsRunning = false;
        }

        private void TimerHandler(object state)
        {
            _timer.Change(-1, -1);
            Run();
            if (RunOnlyOnce)
            {
                Dispose();
            }
            else
            {
                if (FixedStartTime.HasValue)
                {
                    // Make sure the next run is in the future, but is a multiple of the interval after the last start.
                    var nextRun = FixedStartTime.Value.AddSeconds(Seconds);
                    while (nextRun <= DateTime.UtcNow)
                    {
                        nextRun = nextRun.AddSeconds(Seconds);
                    }

                    // Get the number of seconds until the next scheduled interval.
                    var timeToNext = FixedStartTime.Value.AddSeconds(Seconds) - DateTime.UtcNow;
                    _timer.Change(timeToNext, new TimeSpan(0, 0, 0, Seconds));
                }
                else
                {
                    _timer.Change(new TimeSpan(0, 0, 0, Seconds), new TimeSpan(0, 0, 0, Seconds));
                }
            }
        }

        public void Dispose()
        {
            if (_timer != null && !_disposed)
            {
                lock (this)
                {
                    _timer.Dispose();
                    _timer = null;
                    _disposed = true;
                }
            }
        }

        public void InitTimer()
        {
            if (_timer == null)
            {
                if (FixedStartTime.HasValue)
                {
                    // Make sure the next run is in the future, but is a multiple of the interval after the last start.
                    var nextRun = FixedStartTime.Value.AddSeconds(Seconds);
                    while (nextRun <= DateTime.UtcNow)
                    {
                        nextRun = nextRun.AddSeconds(Seconds);
                    }

                    // Get the number of seconds until the next scheduled interval.
                    var timeToNext = FixedStartTime.Value.AddSeconds(Seconds) - DateTime.UtcNow;
                    _timer = new Timer(TimerHandler, null, timeToNext, new TimeSpan(0, 0, 0, Seconds));
                }
                else
                {
                    _timer = new Timer(TimerHandler, null, new TimeSpan(0, 0, 0, Seconds), new TimeSpan(0, 0, 0, Seconds));
                }
            }
        }

        public void AddTask(ScheduledTask task)
        {
            if (!_tasks.ContainsKey(task.Name))
            {
                _tasks.Add(task.Name, task.Type);
            }
        }
    }
}
