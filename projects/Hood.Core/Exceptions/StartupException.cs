using System;
using System.Runtime.Serialization;
using Hood.Enums;

namespace Hood.Core
{
    [Serializable]
    public class StartupException : Exception
    {
        public StartupError Error { get; set; }

        public StartupException(string message, StartupError error) : base(message)
        {
            ProcessError(error);
        }

        public StartupException(string message, Exception innerException, StartupError error) : base(message, innerException)
        {
            ProcessError(error);
        }

        protected StartupException(SerializationInfo info, StreamingContext context, StartupError error) : base(info, context)
        {
            ProcessError(error);
        }

        private void ProcessError(StartupError error)
        {
            Error = error;
            Engine.Services.StartupExceptions.Add(this);
        }
    }
}
