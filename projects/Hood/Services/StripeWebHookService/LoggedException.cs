using Hood.Models;
using System;
using System.Runtime.Serialization;

namespace Hood.Services
{
    [Serializable]
    internal class LoggedException : Exception
    {
        public LogType LogType { get; set; }
        public LoggedException()
        {
        }

        public LoggedException(string message, LogType logType = LogType.Warning) : base(message)
        {
            LogType = logType;
        }

        public LoggedException(string message, Exception innerException, LogType logType = LogType.Warning) : base(message, innerException)
        {
            LogType = logType;
        }

        protected LoggedException(SerializationInfo info, StreamingContext context, LogType logType = LogType.Warning) : base(info, context)
        {
            LogType = logType;
        }
    }
}