using Hood.Enums;
using Hood.Models;
using System;
using System.Runtime.Serialization;

namespace Hood.Services
{
    [Serializable]
    internal class AlertedException : Exception
    {
        public LogType LogType { get; set; }
        public AlertedException()
        {
        }

        public AlertedException(string message, LogType logType = LogType.Error) : base(message)
        {
            LogType = logType;
        }

        public AlertedException(string message, Exception innerException, LogType logType = LogType.Error) : base(message, innerException)
        {
            LogType = logType;
        }

        protected AlertedException(SerializationInfo info, StreamingContext context, LogType logType = LogType.Error) : base(info, context)
        {
            LogType = logType;
        }
    }
}